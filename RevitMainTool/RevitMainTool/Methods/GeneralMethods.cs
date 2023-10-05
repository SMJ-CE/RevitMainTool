using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.UI;
using QuickGraph.Algorithms.ConnectedComponents;
using QuickGraph;

namespace RevitMainTool
{
    public static class GeneralMethods
    {
        public static XYZ GetMidpointOfElementByLocation(Location location)
        {
            XYZ hostLocation = null;

            if (location is LocationPoint locPoint)
            {
                hostLocation = locPoint.Point;
            }
            else if (location is LocationCurve locCurve)
            {
                XYZ startPoint = locCurve.Curve.GetEndPoint(0);
                XYZ endPoint = locCurve.Curve.GetEndPoint(1);
                hostLocation = startPoint.Add(endPoint.Subtract(startPoint).Divide(2));
            }

            return hostLocation;
        }

        public static double GetRotationOfElement(Location location)
        {
            double hostLocation = 0;

            if (location is LocationPoint locPoint)
            {
                hostLocation = locPoint.Rotation;
            }
            else if (location is LocationCurve locCurve)
            {
                XYZ direction = (locCurve.Curve as Line).Direction;
                XYZ directionMade2D = new XYZ(direction.X, direction.Y, 0);
                XYZ yAxis = new XYZ(1, 0, 0);

                hostLocation = Math.Acos(directionMade2D.DotProduct(yAxis));
            }

            return hostLocation;
        }



        public static List<List<Element>> GroupElementsByBoundingBox(List<Element> elements, View view)
        {
            List<List<Element>> groups = new List<List<Element>>();
            Dictionary<ElementId, int> elementToGroupMapping = new Dictionary<ElementId, int>();

            // Assign each element to a new group
            for (int i = 0; i < elements.Count; i++)
            {
                groups.Add(new List<Element>() { elements[i] });
                elementToGroupMapping.Add(elements[i].Id, i);
            }

            // Iterate through each pair of elements to check for overlaps
            for (int i = 0; i < elements.Count; i++)
            {
                for (int j = i + 1; j < elements.Count; j++)
                {
                    // Skip if the elements are the same or already in the same group
                    if (elementToGroupMapping[elements[i].Id] == elementToGroupMapping[elements[j].Id])
                    {
                        continue;
                    }

                    // Check for spatial overlap using the geometry of elements (you may need to refine this based on your specific needs)
                    if (DoBoundingBoxesOverlap(elements[i], elements[j], view))
                    {
                        // If the elements overlap, merge the groups
                        int groupIndex1 = elementToGroupMapping[elements[i].Id];
                        int groupIndex2 = elementToGroupMapping[elements[j].Id];

                        // Merge the groups
                        groups[groupIndex1].AddRange(groups[groupIndex2]);
                        groups[groupIndex2].Clear();

                        // Update the mapping for all elements in the merged group
                        foreach (Element mergedElement in groups[groupIndex1])
                        {
                            elementToGroupMapping[mergedElement.Id] = groupIndex1;
                        }
                    }
                }
            }

            // Remove empty groups
            groups.RemoveAll(group => group.Count == 0);

            return groups;
        }


        public static bool DoBoundingBoxesOverlap(Element ele1, Element ele2, View view)
        {
            BoundingBoxXYZ box1 = ele1.get_BoundingBox(view);
            BoundingBoxXYZ box2 = ele2.get_BoundingBox(view);

            // Check if box1 is entirely to the left of box2
            if (box1.Max.X < box2.Min.X || box2.Max.X < box1.Min.X)
                return false;

            // Check if box1 is entirely above box2
            if (box1.Max.Y < box2.Min.Y || box2.Max.Y < box1.Min.Y)
                return false;

            // Check if box1 is entirely in front of box2
            if (box1.Max.Z < box2.Min.Z || box2.Max.Z < box1.Min.Z)
                return false;

            // If none of the above conditions are met, the bounding boxes overlap
            return true;
        }


        static bool DoBoundingBoxesOverlap(BoundingBoxXYZ box1, BoundingBoxXYZ box2)
        {
            // Check if box1 is entirely to the left of box2
            if (box1.Max.X < box2.Min.X || box2.Max.X < box1.Min.X)
                return false;

            // Check if box1 is entirely above box2
            if (box1.Max.Y < box2.Min.Y || box2.Max.Y < box1.Min.Y)
                return false;

            // Check if box1 is entirely in front of box2
            if (box1.Max.Z < box2.Min.Z || box2.Max.Z < box1.Min.Z)
                return false;

            // If none of the above conditions are met, the bounding boxes overlap
            return true;
        }



        /// <summary>
        /// Output is this Array: gridTop, gridUnder, gridLeft, gridRight 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static Grid[] GetClosestGridLines(Element element)
        {
            Document doc = element.Document;
            XYZ elementPoint;
            var elementLocation = element.Location;

            if(elementLocation is LocationPoint locPoint)
            {
                elementPoint = locPoint.Point;
            }
            else if(elementLocation is LocationCurve locCurve)
            {
                XYZ start = locCurve.Curve.GetEndPoint(0);
                XYZ end = locCurve.Curve.GetEndPoint(1);

                elementPoint = start.Add(end.Subtract(start));
            }
            else
            {
                return null;
            }

            var test = GetClosestGridLines(elementPoint, doc);

            return test;
        }

        public static Grid[] GetClosestGridLines(XYZ elementPoint, Document doc)
        {
            View view = doc.ActiveView;

            var grids = new FilteredElementCollector(doc, view.Id).OfCategory(BuiltInCategory.OST_Grids).Cast<Grid>().ToList();

            Grid gridTop = null;
            Grid gridUnder = null;
            Grid gridLeft = null;
            Grid gridRight = null;

            foreach (Grid grid in grids)
            {
                Line line = grid.Curve as Line;

                XYZ gridPoint = line.GetEndPoint(0);

                double xDir = Math.Round(line.Direction.X);
                double yDir = Math.Round(line.Direction.Y);

                if (yDir == 1 || yDir == -1)
                {
                    double xGridPoint = gridPoint.X;
                    double xElePoint = elementPoint.X;

                    if (gridLeft == null)
                    {
                        if (xGridPoint < xElePoint)
                        {
                            gridLeft = grid;
                        }

                    }
                    else if (xGridPoint < xElePoint)
                    {
                        var test = (gridLeft.Curve as Line).GetEndPoint(0).X;

                        if (test < xGridPoint)
                        {
                            gridLeft = grid;
                        }
                    }

                    if (gridRight == null)
                    {
                        if (xGridPoint > xElePoint)
                        {
                            gridRight = grid;
                        }

                    }
                    else if (xGridPoint > xElePoint)
                    {
                        var test = (gridRight.Curve as Line).GetEndPoint(0).X;

                        if (test > xGridPoint)
                        {
                            gridRight = grid;
                        }
                    }

                }
                else if (xDir == 1 || xDir == -1)
                {
                    double yGridPoint = gridPoint.Y;
                    double yElePoint = elementPoint.Y;

                    if (gridUnder == null)
                    {
                        if (yGridPoint < yElePoint)
                        {
                            gridUnder = grid;
                        }

                    }
                    else if (yGridPoint < yElePoint)
                    {
                        var test = (gridUnder.Curve as Line).GetEndPoint(0).Y;

                        if (test < yGridPoint)
                        {
                            gridUnder = grid;
                        }
                    }

                    if (gridTop == null)
                    {
                        if (yGridPoint > yElePoint)
                        {
                            gridTop = grid;
                        }

                    }
                    else if (yGridPoint > yElePoint)
                    {
                        var currentGridX = (gridTop.Curve as Line).GetEndPoint(0).Y;

                        if (currentGridX > yGridPoint)
                        {
                            gridTop = grid;
                        }
                    }
                }
            }

            return new Grid[4] { gridTop, gridUnder, gridLeft, gridRight };

        }

        public static ICollection<Element> GetSimilarInstances(Element instance)
        {
            Document doc = instance.Document;
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector = collector.OfClass(typeof(FamilySymbol));

            var query = from element in collector where element.Name == instance.Name select element;
            List<Element> famSyms = query.ToList<Element>();
            ElementId symbolId = famSyms[0].Id;

            FamilyInstanceFilter filter = new FamilyInstanceFilter(doc, symbolId);

            collector = new FilteredElementCollector(doc);
            return collector.WherePasses(filter).ToElements();
        }

        public static BoundingBoxXYZ GetBoundingBox(List<BoundingBoxXYZ> boundingBoxes)
        {
            if (boundingBoxes.Count == 0)
            {
                return null;
            }

            double maxX = double.MinValue;
            double maxY = double.MinValue;
            double minX = double.MaxValue;
            double minY = double.MaxValue;

            foreach (BoundingBoxXYZ bound in boundingBoxes)
            {
                maxX = Math.Max(maxX, bound.Max.X);
                maxY = Math.Max(maxY, bound.Max.Y);
                minX = Math.Min(minX, bound.Min.X);
                minY = Math.Min(minY, bound.Min.Y);
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


        public static BoundingBoxXYZ GetBoundingBox(List<Element> elements)
        {
            View view = elements[0].Document.ActiveView;
            List<XYZ> maxPoints = new List<XYZ>();
            List<XYZ> minPoints = new List<XYZ>();

            double maxX = double.MinValue;
            double maxY = double.MinValue;
            double minX = double.MaxValue;
            double minY = double.MaxValue;

            foreach (Element element in elements)
            {
                BoundingBoxXYZ bound = element.get_BoundingBox(view);
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

            BoundingBoxXYZ bounding = new BoundingBoxXYZ();
            bounding.Min = min;
            bounding.Max = max;

            return bounding;
        }
    }
}
