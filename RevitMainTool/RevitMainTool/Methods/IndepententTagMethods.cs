using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using MoreLinq;

namespace RevitMainTool
{

    public static class IndepententTagMethods
    {
        public static void TagAllFamiliesSimilar(Element element)
        {
            Document doc = element.Document;

            if (element is IndependentTag tag)
            {
                ElementId viewId = tag.OwnerViewId;
                View view = doc.GetElement(viewId) as View;

                Element hostElement = doc.GetElement(tag.GetTaggedReferences().FirstOrDefault().ElementId);
                Location hostElementLocation = hostElement.Location;
                XYZ hostMidpoint = GeneralMethods.GetMidpointOfElementByLocation(hostElementLocation);
                Double hostRotation = GeneralMethods.GetRotationOfElement(hostElementLocation);
                BuiltInCategory hostCategory = hostElement.Category.BuiltInCategory;
                ElementId familyId = hostElement.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsElementId();

                XYZ tagLocation = GetMidPointOfElementByBoundingBox(tag, view);
                bool leader = tag.HasLeader;
                TagOrientation originalOrientation = tag.TagOrientation;
                ElementId symbolId = tag.GetTypeId();

                XYZ translation = tagLocation.Subtract(hostMidpoint);
                double orientationInverter = 0;

                if (!(Math.Round(Math.Round(hostRotation, 2) % Math.Round(Math.PI, 2), 2) == 0 ^ originalOrientation == TagOrientation.Horizontal))
                {
                    orientationInverter = Math.Round(Math.PI, 2) / 2;
                }

                var tagsInView = new FilteredElementCollector(doc, viewId).OfCategory(tag.Category.BuiltInCategory).Where(x => x is IndependentTag).Select(x => (IndependentTag)x).ToList();
                var allTaggedElements = tagsInView.Select(x => x.GetTaggedElementIds().First().HostElementId).ToList();

                var allSimilarElements = new FilteredElementCollector(doc, viewId).OfCategory(hostCategory).ToList().FindAll(x => x.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsElementId() == familyId);
                var allSimilarElementsInViewIds = allSimilarElements.Select(x => x.Id).ToList();

                foreach (ElementId eleId in allSimilarElementsInViewIds)
                {
                    int indexInList = allTaggedElements.IndexOf(eleId);

                    Element currentElement = doc.GetElement(eleId);
                    Location currentElementLocation = currentElement.Location;

                    XYZ currentMidpoint = GeneralMethods.GetMidpointOfElementByLocation(currentElementLocation);
                    Double currentRotation = GeneralMethods.GetRotationOfElement(currentElementLocation);

                    Double finalRotation = currentRotation - hostRotation;
                    XYZ rotatedTranslation = Rotate(translation, finalRotation);

                    XYZ finalHeadLocation = currentMidpoint.Add(rotatedTranslation);

                    TagOrientation tagOrientation = TagOrientation.Horizontal;

                    if (Math.Round(Math.Round(currentRotation, 2) % Math.Round(Math.PI, 2), 2) == orientationInverter)
                    {
                        tagOrientation = TagOrientation.Vertical;
                    }

                    if (indexInList == -1)
                    {
                        IndependentTag newTag;
                        //Create Tag
                        using (var tx = new Transaction(doc))
                        {
                            tx.Start("Create Tag");

                            newTag = IndependentTag.Create(doc, symbolId, viewId, new Reference(currentElement), leader, tagOrientation, finalHeadLocation);

                            tx.Commit();
                        }
                        PlaceTagByMidpoint(newTag, finalHeadLocation, view);
                    }
                    else
                    {
                        IndependentTag tagOfElement = tagsInView[indexInList];

                        using (var tx = new Transaction(doc))
                        {
                            tx.Start("Edit Tag");

                            tagOfElement.ChangeTypeId(symbolId);
                            tagOfElement.TagOrientation = tagOrientation;
                            tagOfElement.HasLeader = leader;

                            tx.Commit();
                        }

                        PlaceTagByMidpoint(tagOfElement, finalHeadLocation, view);

                    }
                }
            }
        }

        public static XYZ GetMidPointOfElementByBoundingBox(Element element, View view)
        {
            BoundingBoxXYZ bounding;
            if (element is IndependentTag)
            {
                bounding = GetTagBoundingBox(element as IndependentTag, view);
            }
            else
            {
                bounding = element.get_BoundingBox(view);
            }

            XYZ max = bounding.Max;
            XYZ min = bounding.Min;

            XYZ middlePoint = min.Add(max.Subtract(min) / 2);

            return middlePoint;
        }

        public static XYZ Rotate(XYZ vector, Double radians)
        {
            double x = vector.X;
            double y = vector.Y;

            double nX = x * Math.Cos(radians) - y * Math.Sin(radians);
            double nY = x * Math.Sin(radians) + y * Math.Cos(radians);

            return new XYZ(nX, nY, vector.Z);
        }


        public static void TagFamily(FamilyInstance instance)
        {
            XYZ direction = instance.FacingOrientation.Normalize();
            double length = 1;
            Document doc = instance.Document;
            XYZ familyLocationPoint = (instance.Location as LocationPoint).Point;
            View view = doc.ActiveView;
            ElementId symbolId = GetMostCommonTagFromFamilyInstance(instance);
            TagOrientation orientation = TagOrientation.Vertical;

            if (Math.Round(direction.X, 2) != 0)
            {
                orientation = TagOrientation.Horizontal;
            }

            IndependentTag theTag;

            using (var tx = new Transaction(doc))
            {
                tx.Start("Create Tag");

                theTag = IndependentTag.Create(doc, symbolId, view.Id, new Reference(instance), false, orientation, familyLocationPoint);

                tx.Commit();
            }


            BoundingBoxXYZ tagBoundingBox = theTag.get_BoundingBox(view);

            switch (orientation)
            {
                case TagOrientation.Horizontal:
                    double lengthX = tagBoundingBox.Max.X - tagBoundingBox.Min.X;
                    length += lengthX / 2;
                    break;
                case TagOrientation.Vertical:
                    double lengthY = tagBoundingBox.Max.Y - tagBoundingBox.Min.Y;
                    length += lengthY / 2;
                    break;
            }

            XYZ endPoint = familyLocationPoint.Add(direction.Multiply(length));

            //PlaceTagByMidpoint(theTag, endPoint);
        }

        public static void PlaceTagByMidpoint(IndependentTag tag, XYZ newMidpoint, View view)
        {
            BoundingBoxXYZ box = GetTagBoundingBox(tag, view);
            XYZ difference = box.Max.Subtract(box.Min).Divide(2);
            XYZ newMaxPoint = newMidpoint.Add(difference);
            MoveTagByBoundingboxMax(tag, newMaxPoint, view);
        }

        public static XYZ PointFromFeetToMM(XYZ point)
        {
            double conversionactor = 304.8;
            return new XYZ(point.X * conversionactor, point.Y * conversionactor, point.Z * conversionactor);
        }

        public static ElementId GetMostCommonTagFromFamilyInstance(FamilyInstance instance)
        {
            Document doc = instance.Document;
            List<IndependentTag> independentTags = GetMostReleventTagsFromFamilyInstance(instance);

            IndependentTag mostCommon = independentTags.MostCommon();

            ElementId eleid = mostCommon.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsElementId();
            FamilySymbol element = doc.GetElement(eleid) as FamilySymbol;

            return element.Id;
        }

        public static List<IndependentTag> GetMostReleventTagsFromFamilyInstance(FamilyInstance instance)
        {

            Document doc = instance.Document;
            string instanceCat = instance.Category.BuiltInCategory.ToString();
            if (instanceCat.EndsWith("s"))
            {
                instanceCat = instanceCat.Remove(instanceCat.Length - 1);
            }

            BuiltInCategory tagCategory = (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), instanceCat + "Tags");
            var allDemTags = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(tagCategory);

            if (allDemTags.Count() == 0)
            {
                allDemTags = new FilteredElementCollector(doc).OfCategory(tagCategory);
            }

            List<IndependentTag> independentTags = new List<IndependentTag>();
            ICollection<Element> familyInstances = GeneralMethods.GetSimilarInstances(instance);
            List<ElementId> familyInstancesId = new List<ElementId>();

            foreach (Element familyInstance in familyInstances)
            {
                familyInstancesId.Add(familyInstance.Id);
            }

            foreach (var t in allDemTags)
            {
                if (t is IndependentTag tag)
                {
                    if (familyInstancesId.Contains(tag.GetTaggedElementIds().First().HostElementId))
                    {
                        independentTags.Add(tag);
                    }

                }
            }

            if (independentTags.Count() == 0)
            {
                allDemTags = new FilteredElementCollector(doc).OfCategory(tagCategory);
                foreach (var t in allDemTags)
                {
                    if (t is IndependentTag tag)
                    {
                        independentTags.Add(tag);
                    }
                }
            }

            if (independentTags.Count() == 0)
            {
                return null;
            }

            return independentTags;
        }

        public static void AlignTagsRight(List<IndependentTag> tags)
        {
            if (tags.Count == 0)
            {
                return;
            }

            View currentView = ViewMethods.GetCurrentViewFromElement(tags.First());

            IndependentTag mostRightTag = GetMostRightTag(tags, currentView);
            double rightX = GetTagBoundingBox(mostRightTag, currentView).Max.X;

            foreach (IndependentTag tag in tags)
            {
                BoundingBoxXYZ bound = GetTagBoundingBox(tag, currentView);
                XYZ oldPoint = bound.Max;
                XYZ newPoint = new XYZ(rightX, oldPoint.Y, oldPoint.Z);
                MoveTagByBoundingboxMax(tag, newPoint, currentView);
            }
        }

        public static void AlignTagsLeft(List<IndependentTag> tags)
        {
            if (tags.Count == 0)
            {
                return;
            }

            View currentView = ViewMethods.GetCurrentViewFromElement(tags.First());

            IndependentTag mostLeftTag = GetMostLeftTag(tags, currentView);
            double leftX = GetTagBoundingBox(mostLeftTag, currentView).Min.X;

            foreach (IndependentTag tag in tags)
            {
                BoundingBoxXYZ bound = GetTagBoundingBox(tag, currentView);
                XYZ oldPoint = bound.Min;
                XYZ newPoint = new XYZ(leftX, oldPoint.Y, oldPoint.Z);
                MoveTagByBoundingboxMin(tag, newPoint, currentView);
            }
        }

        public static BoundingBoxXYZ GetTagBoundingBox(List<IndependentTag> tags, View view)
        {
            List<XYZ> maxPoints = new List<XYZ>();
            List<XYZ> minPoints = new List<XYZ>();

            double maxX = double.MinValue;
            double maxY = double.MinValue;
            double minX = double.MaxValue;
            double minY = double.MaxValue;

            foreach (IndependentTag tag in tags)
            {
                BoundingBoxXYZ bound = GetTagBoundingBox(tag, view);
                maxPoints.Add(bound.Max);
                minPoints.Add(bound.Min);
            }

            for (int i = 0; i < maxPoints.Count; i++)
            {
                XYZ maxPoint = maxPoints[i];
                XYZ minPoint = minPoints[i];

                double maxPointX = maxPoint.X;
                double maxPointY = maxPoint.Y;
                double minPointX = minPoint.X;
                double minPointY = minPoint.Y;

                if (maxX < maxPointX)
                {
                    maxX = maxPointX;
                }
                if (maxY < maxPointY)
                {
                    maxY = maxPointY;
                }
                if (minX > minPointX)
                {
                    minX = minPointX;
                }
                if (minY > minPointY)
                {
                    minY = minPointY;
                }
            }

            XYZ max = new XYZ(maxX, maxY, 0);
            XYZ min = new XYZ(minX, minY, 0);

            BoundingBoxXYZ bounding = new BoundingBoxXYZ
            {
                Min = min,
                Max = max
            };

            return bounding;
        }

        public static BoundingBoxXYZ GetTagBoundingBox(IndependentTag tag, View view)
        {
            Document doc = tag.Document;

            //Dimension to return
            BoundingBoxXYZ tagBoundingBox = null;

            Reference someReference = tag.GetTaggedReferences().First();

            using (TransactionGroup transG = new TransactionGroup(doc))
            {
                transG.Start("Determine Tag Dimension");

                XYZ existingTagHeadPosition = tag.TagHeadPosition;
                XYZ newTagHeadPosition;

                if (!tag.HasLeader)
                {
                    using (Transaction trans = new Transaction(doc))
                    {
                        trans.Start("Activate Leader");

                        tag.HasLeader = true;

                        trans.Commit();
                    }
                }

                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Determine Tag Dimension");

                    tag.LeaderEndCondition = LeaderEndCondition.Free;
                    newTagHeadPosition = tag.GetLeaderEnd(someReference);
                    tag.TagHeadPosition = newTagHeadPosition;
                    tag.SetLeaderElbow(someReference, newTagHeadPosition);

                    trans.Commit();
                }

                //Tag Dimension
                XYZ difference = existingTagHeadPosition.Subtract(newTagHeadPosition);
                BoundingBoxXYZ tagBox = tag.get_BoundingBox(view);

                tagBoundingBox = new BoundingBoxXYZ()
                {
                    Min = tagBox.Min.Add(difference),
                    Max = tagBox.Max.Add(difference)
                };

                transG.RollBack();
            }

            return tagBoundingBox;
        }

        public static Tuple<double, double> GetTagExtents(IndependentTag tag)
        {
            Document doc = tag.Document;

            //Dimension to return
            double tagWidth;
            double tagHeight;

            //Tag's View and Element
            View sec = doc.GetElement(tag.OwnerViewId) as View;
            XYZ rightDirection = sec.RightDirection;
            XYZ upDirection = sec.UpDirection;
            Reference pipeReference = tag.GetTaggedReferences().First();

            using (TransactionGroup transG = new TransactionGroup(doc))
            {
                transG.Start("Determine Tag Dimension");

                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Determine Tag Dimension");
                    tag.HasLeader = true;

                    tag.LeaderEndCondition = LeaderEndCondition.Free;
                    XYZ leaderEndPoint = tag.GetLeaderEnd(pipeReference);
                    tag.TagHeadPosition = leaderEndPoint;
                    tag.SetLeaderElbow(pipeReference, leaderEndPoint);

                    trans.Commit();
                }

                //Tag Dimension
                BoundingBoxXYZ tagBox = tag.get_BoundingBox(sec);
                tagWidth = (tagBox.Max - tagBox.Min).DotProduct(rightDirection);
                tagHeight = (tagBox.Max - tagBox.Min).DotProduct(upDirection);

                transG.RollBack();
            }
            return Tuple.Create(tagWidth, tagHeight);
        }

        public static void AlignTagsCenter(List<IndependentTag> tags)
        {
            if (tags.Count == 0)
            {
                return;
            }

            View currentView = ViewMethods.GetCurrentViewFromElement(tags.First());

            IndependentTag mostLeftTag = GetMostLeftTag(tags, currentView);
            double leftX = mostLeftTag.TagHeadPosition.X;

            foreach (IndependentTag tag in tags)
            {
                XYZ newPoint = new XYZ(leftX, tag.TagHeadPosition.Y, tag.TagHeadPosition.Z);
                MoveTagHeadToPointTX(tag, newPoint);
            }
        }

        public static IndependentTag GetMostLeftTag(List<IndependentTag> tags, View view)
        {
            if (tags.Count == 0)
            {
                return null;
            }
            if (view == null)
            {
                return tags.Minima(tag => tag.TagHeadPosition.X).First();
            }
            else
            {
                return tags.Minima(tag => GetTagBoundingBox(tag, view).Min.X).First();
            }
        }

        public static IndependentTag GetMostRightTag(List<IndependentTag> tags, View view)
        {
            if (tags.Count == 0)
            {
                return null;
            }
            if (view == null)
            {
                return tags.Maxima(tag => tag.TagHeadPosition.X).First();
            }
            else
            {
                return tags.Maxima(tag => GetTagBoundingBox(tag, view).Max.X).First();
            }
        }

        public static XYZ TagGetEndPointOfLeader(IndependentTag tag)
        {
            Document doc = tag.Document;
            LeaderEndCondition leaderEnd = tag.LeaderEndCondition;
            XYZ thePoint;
            if (tag.HasLeader)
            {
                if (leaderEnd == LeaderEndCondition.Attached)
                {
                    using (Transaction trans = new Transaction(doc))
                    {
                        trans.Start("Get Lead End");
                        tag.LeaderEndCondition = LeaderEndCondition.Free;
                        thePoint = tag.GetLeaderEnd(tag.GetTaggedReferences().First());
                        trans.RollBack();
                    }
                }
                else
                {
                    thePoint = tag.GetLeaderEnd(tag.GetTaggedReferences().First());
                }
            }
            else
            {
                Element ele = doc.GetElement(tag.GetTaggedReferences().First().ElementId);
                thePoint = (ele.Location as LocationPoint).Point;
            }


            return thePoint;
        }

        public static void SpaceTagsFixedDistanceVerticale(List<IndependentTag> tags, double distanceBetweenTags, Alignment alignment)
        {
            // Sort tags based on their combined Y and X positions.
            tags = tags.OrderBy(p => TagGetEndPointOfLeader(p).Y).ToList();
            // Get the active view from the first tag's document.
            Document doc = tags[0].Document;
            View view = doc.ActiveView;

            foreach (IndependentTag tag in tags)
            {
                if (tag.TagOrientation == TagOrientation.Vertical)
                {
                    using (Transaction trans = new Transaction(doc))
                    {
                        trans.Start("Change Tag Orientation");
                        tag.TagOrientation = TagOrientation.Horizontal;
                        trans.Commit();
                    }
                }
            }

            // Initialize variables for total distance and bounding boxes.
            double totalDistance = 0;
            List<BoundingBoxXYZ> boundingBoxes = new List<BoundingBoxXYZ>();

            // Calculate total distance and collect bounding boxes.
            foreach (IndependentTag tag in tags)
            {
                BoundingBoxXYZ bounding = GetTagBoundingBox(tag, view);
                boundingBoxes.Add(bounding);
                double tagHeight = bounding.Max.Y - bounding.Min.Y;
                totalDistance += tagHeight;
            }

            // Add the vertical spacing between tags.
            totalDistance += distanceBetweenTags * (tags.Count - 1);

            // Calculate the midpoint and starting point.
            BoundingBoxXYZ boundingBox = GeneralMethods.GetBoundingBox(boundingBoxes);
            double halfDistance = totalDistance / 2;
            XYZ totalMin = boundingBox.Min;
            XYZ totalMax = boundingBox.Max;
            XYZ totalMid = totalMin.Add(totalMax.Subtract(totalMin).Divide(2));
            XYZ startPoint;
            double distanceSoFar = 0;

            switch (alignment)
            {
                case Alignment.Left:
                    startPoint = new XYZ(totalMin.X, totalMin.Y, totalMin.Z);

                    foreach (IndependentTag tag in tags)
                    {
                        BoundingBoxXYZ bounding = boundingBoxes[tags.IndexOf(tag)];
                        double tagHeight = bounding.Max.Y - bounding.Min.Y;
                        XYZ newPoint = new XYZ(startPoint.X, startPoint.Y + distanceSoFar, startPoint.Z);
                        MoveTagByBoundingboxMin(tag, newPoint, view);
                        distanceSoFar += tagHeight + distanceBetweenTags;
                    }
                    break;
                case Alignment.Right:
                    startPoint = new XYZ(totalMax.X, totalMin.Y, totalMax.Z);

                    foreach (IndependentTag tag in tags)
                    {
                        BoundingBoxXYZ bounding = boundingBoxes[tags.IndexOf(tag)];
                        double tagHeight = bounding.Max.Y - bounding.Min.Y;
                        distanceSoFar += tagHeight;
                        XYZ newPoint = new XYZ(startPoint.X, startPoint.Y + distanceSoFar, startPoint.Z);
                        MoveTagByBoundingboxMax(tag, newPoint, view);
                        distanceSoFar += distanceBetweenTags;
                    }
                    break;
                case Alignment.Center:
                    startPoint = new XYZ(totalMid.X, totalMid.Y - halfDistance, totalMid.Z);

                    foreach (IndependentTag tag in tags)
                    {
                        BoundingBoxXYZ bounding = boundingBoxes[tags.IndexOf(tag)];
                        double tagHeight = bounding.Max.Y - bounding.Min.Y;
                        XYZ newPoint = new XYZ(startPoint.X, startPoint.Y + distanceSoFar + tagHeight / 2, startPoint.Z);
                        PlaceTagByMidpoint(tag, newPoint, view);
                        distanceSoFar += tagHeight + distanceBetweenTags;
                    }
                    break;
                default:
                    goto case Alignment.Center;
            }
        }

        public static void SpaceTagsFixedDistanceHorizontal(List<IndependentTag> tags, double distanceBetweenTags, Alignment alignment)
        {
            // Sort tags based on their combined Y and X positions.
            tags = tags.OrderBy(p => TagGetEndPointOfLeader(p).X).ToList();
            // Get the active view from the first tag's document.
            Document doc = tags[0].Document;
            View view = doc.ActiveView;

            foreach (IndependentTag tag in tags)
            {
                if (tag.TagOrientation == TagOrientation.Horizontal)
                {
                    using (Transaction trans = new Transaction(doc))
                    {
                        trans.Start("Change Tag Orientation");
                        tag.TagOrientation = TagOrientation.Vertical;
                        trans.Commit();
                    }
                }
            }

            // Initialize variables for total distance and bounding boxes.
            double totalDistance = 0;
            List<BoundingBoxXYZ> boundingBoxes = new List<BoundingBoxXYZ>();

            // Calculate total distance and collect bounding boxes.
            foreach (IndependentTag tag in tags)
            {
                BoundingBoxXYZ bounding = GetTagBoundingBox(tag, view);
                boundingBoxes.Add(bounding);
                double tagHeight = bounding.Max.X - bounding.Min.X;
                totalDistance += tagHeight;
            }

            // Add the vertical spacing between tags.
            totalDistance += distanceBetweenTags * (tags.Count - 1);

            // Calculate the midpoint and starting point.
            BoundingBoxXYZ boundingBox = GeneralMethods.GetBoundingBox(boundingBoxes);
            double halfDistance = totalDistance / 2;
            XYZ totalMin = boundingBox.Min;
            XYZ totalMax = boundingBox.Max;
            XYZ totalMid = totalMin.Add(totalMax.Subtract(totalMin).Divide(2));
            XYZ startPoint;
            double distanceSoFar = 0;

            switch (alignment)
            {
                case Alignment.Left:
                    startPoint = new XYZ(totalMin.X, totalMin.Y, totalMin.Z);

                    foreach (IndependentTag tag in tags)
                    {
                        BoundingBoxXYZ bounding = boundingBoxes[tags.IndexOf(tag)];
                        double tagHeight = bounding.Max.X - bounding.Min.X;
                        XYZ newPoint = new XYZ(startPoint.X + distanceSoFar, startPoint.Y, startPoint.Z);
                        MoveTagByBoundingboxMin(tag, newPoint, view);
                        distanceSoFar += tagHeight + distanceBetweenTags;
                    }
                    break;
                case Alignment.Right:
                    startPoint = new XYZ(totalMax.X, totalMin.Y, totalMax.Z);

                    foreach (IndependentTag tag in tags)
                    {
                        BoundingBoxXYZ bounding = boundingBoxes[tags.IndexOf(tag)];
                        double tagHeight = bounding.Max.X - bounding.Min.X;
                        distanceSoFar += tagHeight;
                        XYZ newPoint = new XYZ(startPoint.X + distanceSoFar, startPoint.Y, startPoint.Z);
                        MoveTagByBoundingboxMax(tag, newPoint, view);
                        distanceSoFar += distanceBetweenTags;
                    }
                    break;
                case Alignment.Center:
                    startPoint = new XYZ(totalMid.X - halfDistance, totalMid.Y, totalMid.Z);

                    foreach (IndependentTag tag in tags)
                    {
                        BoundingBoxXYZ bounding = boundingBoxes[tags.IndexOf(tag)];
                        double tagHeight = bounding.Max.X - bounding.Min.X;
                        XYZ newPoint = new XYZ(startPoint.X + distanceSoFar + tagHeight / 2, startPoint.Y, startPoint.Z);
                        PlaceTagByMidpoint(tag, newPoint, view);
                        distanceSoFar += tagHeight + distanceBetweenTags;
                    }
                    break;
                default:
                    goto case Alignment.Center;
            }
        }

        public static void SpaceTagsEvenly(List<IndependentTag> tags, XOrY xOrY)
        {
            switch (xOrY)
            {
                case XOrY.XThenY:
                    tags = tags.OrderBy(p => p.TagHeadPosition.X).ThenBy(p => p.TagHeadPosition.Y).ToList();
                    break;
                case XOrY.YThenX:
                    tags = tags.OrderBy(p => p.TagHeadPosition.Y).ThenBy(p => p.TagHeadPosition.X).ToList();
                    break;
                case XOrY.SumOfXandY:
                    tags = tags.OrderBy(p => p.TagHeadPosition.Y + p.TagHeadPosition.X).ToList();
                    break;
            }



            IndependentTag firstTag = tags[0];
            IndependentTag lastTag = tags[tags.Count - 1];

            XYZ firstTagHeadPoint = firstTag.TagHeadPosition;

            XYZ distanceAsPoint = lastTag.TagHeadPosition.Subtract(firstTagHeadPoint);

            XYZ Segment = distanceAsPoint.Divide(tags.Count - 1);

            for (int i = 0; i < tags.Count; i++)
            {
                IndependentTag tag = tags[i];

                var newPoint = firstTagHeadPoint.Add(Segment.Multiply(i));

                MoveTagHeadToPointTX(tag, newPoint);
            }
        }

        public static void MoveTagLocationToPoint(IndependentTag tag, XYZ Point)
        {
            XYZ tagLocation = ((LocationPoint)tag.Location).Point;
            XYZ translation = Point.Subtract(tagLocation);

            tag.Location.Move(translation);
        }

        public static void MoveTagByBoundingboxMin(IndependentTag tag, XYZ Point, View view)
        {
            XYZ pointMin = GetTagBoundingBox(tag, view).Min;
            XYZ pointTagHead = tag.TagHeadPosition;
            XYZ pointDifference = pointTagHead.Subtract(pointMin);

            MoveTagHeadToPointTX(tag, Point.Add(pointDifference));
        }

        public static void MoveTagByBoundingboxMax(IndependentTag tag, XYZ Point, View view)
        {
            XYZ pointMax = GetTagBoundingBox(tag, view).Max;
            XYZ pointTagHead = tag.TagHeadPosition;
            XYZ pointDifference = pointTagHead.Subtract(pointMax);

            MoveTagHeadToPointTX(tag, Point.Add(pointDifference));
        }


        public static void MoveTagHeadToPointTX(IndependentTag tag, XYZ Point)
        {
            using (Transaction trans = new Transaction(tag.Document))
            {
                trans.Start("Move Tag");

                tag.TagHeadPosition = Point;

                //Wiggle to make sure Revit registers the change. Otherwise it doesn't for some reason
                tag.Location.Move(new XYZ(2, 0, 0));
                tag.Location.Move(new XYZ(-2, 0, 0));

                trans.Commit();
            }
        }

        public static void MoveTagHeadToPoint(IndependentTag tag, XYZ Point)
        {
            tag.TagHeadPosition = Point;

            //Wiggle to make sure Revit registers the change. Otherwise it doesn't for some reason
            tag.Location.Move(new XYZ(2, 0, 0));
            tag.Location.Move(new XYZ(-2, 0, 0));
        }
    }
}
