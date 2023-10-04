using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitMainTool.Methods
{
    public class PipeMethods
    {
        public static void DimensionPipeToClosestGrid(Pipe pipe, UIDocument uidoc)
        {
            Document doc = pipe.Document;
            View view = doc.ActiveView;

            if (view is ViewPlan)
            {

                List<Pipe> allPipes = GetPipesThatCutView(uidoc, pipe);

                if (allPipes.Count == 0)
                {
                    TaskDialog.Show("No pipes found", "Couldn't find any pipes that cut the view. Make sure the pipes are visible in the current view and that the view actually cuts it");
                    return;
                }

                Options opt = new Options();
                opt.ComputeReferences = true;
                opt.IncludeNonVisibleObjects = true;
                opt.View = doc.ActiveView;

                List<Dimension> allDimensions = new List<Dimension>();

                foreach (Pipe choosenPipe in allPipes)
                {
                    var grids = GeneralMethods.GetClosestGridLines(choosenPipe);

                    Grid gridTop = grids[0];
                    Grid gridBottom = grids[1];
                    Grid gridLeft = grids[2];
                    Grid gridRight = grids[3];

                    if(gridTop == null && gridBottom == null && gridLeft == null && gridRight == null)
                    {
                        continue;
                    }

                    Curve gridCurveTop = gridTop == null ? null : gridTop.Curve;
                    Curve gridCurveBottom = gridBottom == null ? null : gridBottom.Curve;
                    Curve gridCurveLeft = gridLeft == null ? null : gridLeft.Curve;
                    Curve gridCurveRight = gridRight == null ? null : gridRight.Curve;

                    double offset = 1;

                    Line tesing = (choosenPipe.Location as LocationCurve).Curve as Line;

                    XYZ direction = tesing.Direction;

                    double dirX = direction.X;
                    double dirY = direction.Y;

                    bool isNotVerticale = dirX != 0 || dirY != 0;

                    XYZ originPoint = tesing.Origin;
                    XYZ curveStartPoint = tesing.GetEndPoint(0);
                    XYZ curveEndPoint = tesing.GetEndPoint(1);

                    XYZ newStartPoint = new XYZ(originPoint.X, originPoint.Y, curveStartPoint.Z);
                    XYZ newEndPoint = new XYZ(originPoint.X, originPoint.Y, curveEndPoint.Z);
                    Curve newCurve = Line.CreateBound(newStartPoint, newEndPoint);

                    if (isNotVerticale)
                    {
                        if(Math.Round(dirX, 4) == 0 || Math.Round(dirY, 4) == 0)
                        {
                            using (var tx = new Transaction(doc))
                            {
                                tx.Start("Rotating pipes correctly");

                                (choosenPipe.Location as LocationCurve).Curve = newCurve;

                                tx.Commit();
                            }

                            Line testLeftRight = Line.CreateUnbound(new XYZ(newStartPoint.X, newStartPoint.Y + offset, 0), new XYZ(1, 0, 0));
                            Line testTopBottom = Line.CreateUnbound(new XYZ(newStartPoint.X + offset, newStartPoint.Y, 0), new XYZ(0, 1, 0));

                            ReferenceArray refArrayTopBottom = new ReferenceArray();

                            if(gridTop != null)
                            {
                                refArrayTopBottom.Append(new Reference(gridTop));
                            }
                            refArrayTopBottom.Append(new Reference(choosenPipe));
                            if (gridBottom != null)
                            {
                                refArrayTopBottom.Append(new Reference(gridBottom));
                            }

                            ReferenceArray refArrayLeftRight = new ReferenceArray();

                            if (gridLeft != null)
                            {
                                refArrayLeftRight.Append(new Reference(gridLeft));
                            }

                            refArrayLeftRight.Append(new Reference(choosenPipe));

                            if (gridRight != null)
                            {
                                refArrayLeftRight.Append(new Reference(gridRight));
                            }
                            
                            

                            using (var tx = new Transaction(doc))
                            {
                                tx.Start("Creating Dimensions");

                                if(refArrayLeftRight.Size > 1)
                                {
                                    allDimensions.Add(doc.Create.NewDimension(view, testLeftRight, refArrayLeftRight));
                                }

                                if (refArrayTopBottom.Size > 1)
                                {
                                    allDimensions.Add(doc.Create.NewDimension(view, testTopBottom, refArrayTopBottom));
                                }

                                tx.Commit();
                            }


                        }

                    }
                    else
                    {
                        Line testLeftRight = Line.CreateUnbound(new XYZ(newStartPoint.X, newStartPoint.Y + offset, 0), new XYZ(1, 0, 0));
                        Line testTopBottom = Line.CreateUnbound(new XYZ(newStartPoint.X + offset, newStartPoint.Y, 0), new XYZ(0, 1, 0));

                        ReferenceArray refArrayTopBottom = new ReferenceArray();

                        if (gridTop != null)
                        {
                            refArrayTopBottom.Append(new Reference(gridTop));
                        }
                        refArrayTopBottom.Append(new Reference(choosenPipe));
                        if (gridBottom != null)
                        {
                            refArrayTopBottom.Append(new Reference(gridBottom));
                        }

                        ReferenceArray refArrayLeftRight = new ReferenceArray();

                        if (gridLeft != null)
                        {
                            refArrayLeftRight.Append(new Reference(gridLeft));
                        }

                        refArrayLeftRight.Append(new Reference(choosenPipe));

                        if (gridRight != null)
                        {
                            refArrayLeftRight.Append(new Reference(gridRight));
                        }



                        using (var tx = new Transaction(doc))
                        {
                            tx.Start("Creating Dimensions");

                            if (refArrayLeftRight.Size > 1)
                            {
                                allDimensions.Add(doc.Create.NewDimension(view, testLeftRight, refArrayLeftRight));
                            }

                            if (refArrayTopBottom.Size > 1)
                            {
                                allDimensions.Add(doc.Create.NewDimension(view, testTopBottom, refArrayTopBottom));
                            }

                            tx.Commit();
                        }


                    }

                }

                if (allDimensions.Count > 1)
                {
                    List<List<Dimension>> groupDimensionByDirection = GroupDimensionsByDirection(allDimensions, view);

                    foreach(List<Dimension> dimensions in groupDimensionByDirection)
                    {
                        List<List<Dimension>> groupedDimensions = new List<List<Dimension>>();

                        List<List<Element>> groups = GeneralMethods.GroupElementsByBoundingBox(dimensions.Cast<Element>().ToList(), view).ToList();

                        foreach (List<Element> gro in groups)
                        {
                            groupedDimensions.Add(gro.Cast<Dimension>().ToList());
                        }


                        foreach (List<Dimension> group in groupedDimensions)
                        {
                            ReferenceArray refArray = new ReferenceArray();

                            XYZ origin = (group.First().Curve as Line).Origin;
                            XYZ direction = (group.First().Curve as Line).Direction;

                            List<ElementId> fyribils = new List<ElementId>(); 

                            foreach (Dimension dimension in group)
                            {
                                foreach (Reference re in dimension.References)
                                {
                                    ElementId elId = re.ElementId;


                                    if (!fyribils.Contains(elId))
                                    {
                                        Element el = doc.GetElement(elId);

                                        refArray.Append(new Reference(el));
                                        fyribils.Add(elId);
                                    }
                                }
                            }

                            Line line = Line.CreateUnbound(origin, direction);

                            using (var tx = new Transaction(doc))
                            {
                                tx.Start("Creating Dimensions");

                                doc.Create.NewDimension(view, line, refArray);

                                tx.Commit();
                            }


                        }
                    }
                    using (var tx = new Transaction(doc))
                    {
                        tx.Start("Creating Dimensions");

                        foreach (Dimension dim in allDimensions)
                        {
                            doc.Delete(dim.Id);


                        }


                        tx.Commit();
                    }
                    
                }



            }
            else
            {
                TaskDialog.Show("Wrong View", "Get Your ass to a normal 2D view!!");
            }

        }

        public static List<List<Dimension>> GroupDimensionsByDirection(List<Dimension> dimensions, View view)
        {
            List<List<Dimension>> groups = new List<List<Dimension>>();
            Dictionary<ElementId, int> elementToGroupMapping = new Dictionary<ElementId, int>();

            // Assign each element to a new group
            for (int i = 0; i < dimensions.Count; i++)
            {
                groups.Add(new List<Dimension>() { dimensions[i] });
                elementToGroupMapping.Add(dimensions[i].Id, i);
            }

            // Iterate through each pair of elements to check for overlaps
            for (int i = 0; i < dimensions.Count; i++)
            {
                for (int j = i + 1; j < dimensions.Count; j++)
                {
                    // Skip if the elements are the same or already in the same group
                    if (elementToGroupMapping[dimensions[i].Id] == elementToGroupMapping[dimensions[j].Id])
                    {
                        continue;
                    }

                    // Check for spatial overlap using the geometry of elements (you may need to refine this based on your specific needs)
                    if (DoDimensionsHaveSameDirection(dimensions[i], dimensions[j]))
                    {
                        // If the elements overlap, merge the groups
                        int groupIndex1 = elementToGroupMapping[dimensions[i].Id];
                        int groupIndex2 = elementToGroupMapping[dimensions[j].Id];

                        // Merge the groups
                        groups[groupIndex1].AddRange(groups[groupIndex2]);
                        groups[groupIndex2].Clear();

                        // Update the mapping for all elements in the merged group
                        foreach (Dimension mergedElement in groups[groupIndex1])
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

        public static bool DoDimensionsHaveSameDirection(Dimension dimension1, Dimension dimension2)
        {
            Line line1 = dimension1.Curve as Line;
            Line line2 = dimension2.Curve as Line;

            if (line1.IsBound && line2.IsBound)
            {
                XYZ start1 = dimension1.Curve.GetEndPoint(0);
                XYZ end1 = dimension1.Curve.GetEndPoint(1);

                XYZ start2 = dimension2.Curve.GetEndPoint(0);
                XYZ end2 = dimension2.Curve.GetEndPoint(1);

                double a1 = (end1.Y - start1.Y) / (end1.X - start1.X);
                double a2 = (end2.Y - start2.Y) / (end2.X - start2.X);

                if (Math.Round(a1, 6) == Math.Round(a2, 6))
                {
                    return true;
                }
            }
            else
            {
                double x1 = Math.Round(line1.Direction.X, 6);
                double y1 = Math.Round(line1.Direction.Y, 6);
                double x2 = Math.Round(line2.Direction.X, 6);
                double y2 = Math.Round(line2.Direction.Y, 6);
                
                if (x1 == x2 && y1 == y2)
                {
                    return true;
                }
            }


            

            return false;
        }



        //https://forums.autodesk.com/t5/revit-api-forum/select-elements-witch-cut-by-the-view/m-p/6793155#M20344
        public static List<Pipe> GetPipesThatCutView(UIDocument UIdoc, Element ele)
        {
            Document doc = UIdoc.Document;
            View ActView = doc.ActiveView;

            List<CurveLoop> _crop = ActView.GetCropRegionShapeManager().GetCropShape().ToList<CurveLoop>();
            CurveLoop cvLoop = _crop.First();
            List<XYZ> _cropPoints = new List<XYZ>();
            foreach (Curve cv in cvLoop)
            {
                if (cv is Line)
                {
                    _cropPoints.Add(cv.GetEndPoint(0));
                }
                else
                {
                    // redundant all curves in crop region are straight lines
                    List<XYZ> temp = cv.Tessellate().ToList<XYZ>();
                    temp.RemoveAt(temp.Count - 1);
                    _cropPoints.AddRange(temp);
                }
            }
            if (_cropPoints.Count < 3) return null;
            Outline _outline = new Outline(_cropPoints[0], _cropPoints[1]);
            for (int i = 2; i < _cropPoints.Count; i++)
            {
                _outline.AddPoint(_cropPoints[i]);
            }
            // if you need a bigger region just scale 
            //            _outline.Scale(100);

            string sysAbreviation = ele.get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsValueString();

            BoundingBoxIntersectsFilter CutPlaneFilter = new BoundingBoxIntersectsFilter(_outline);
            // simplefy the result using OfClass or OfCategory
            List<Pipe> pipesInCutPlane =
                   new FilteredElementCollector(doc, doc.ActiveView.Id)
                            .OfClass(typeof(Pipe))
                            .WhereElementIsNotElementType()
                            .WhereElementIsViewIndependent()
                            .WherePasses(CutPlaneFilter)
                            .Where(x => x.get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsValueString() == sysAbreviation)
                            .Cast<Pipe>()
                            .ToList();
            
            return pipesInCutPlane;

        }




    }
}
