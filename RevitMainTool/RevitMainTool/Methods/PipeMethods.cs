using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

                    Curve gridCurveTop = gridTop.Curve;
                    Curve gridCurveBottom = gridBottom.Curve;
                    Curve gridCurveLeft = gridLeft.Curve;
                    Curve gridCurveRight = gridRight.Curve;

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
                        if(Math.Round(dirX, 6) == 0 || Math.Round(dirY, 6) == 0)
                        {
                            using (var tx = new Transaction(doc))
                            {
                                tx.Start("Rotating pipes correctly");

                                (choosenPipe.Location as LocationCurve).Curve = newCurve;

                                tx.Commit();
                            }

                            Line line = Line.CreateBound(new XYZ(gridCurveLeft.GetEndPoint(0).X, newStartPoint.Y + offset, 0), new XYZ(gridCurveRight.GetEndPoint(0).X, newStartPoint.Y + offset, 0));

                            ReferenceArray refArray = new ReferenceArray();
                            refArray.Append(new Reference(choosenPipe));
                            refArray.Append(new Reference(gridLeft));
                            refArray.Append(new Reference(gridRight));

                            using (var tx = new Transaction(doc))
                            {
                                tx.Start("Creating Dimensions");

                                allDimensions.Add(doc.Create.NewDimension(view, line, refArray));

                                tx.Commit();
                            }


                        }

                    }
                    else
                    {
                        Line line = Line.CreateBound(new XYZ(gridCurveLeft.GetEndPoint(0).X, newStartPoint.Y + offset, 0), new XYZ(gridCurveRight.GetEndPoint(0).X, newStartPoint.Y + offset, 0));

                        ReferenceArray refArray = new ReferenceArray();
                        refArray.Append(new Reference(choosenPipe));
                        refArray.Append(new Reference(gridLeft));
                        refArray.Append(new Reference(gridRight));

                        using (var tx = new Transaction(doc))
                        {
                            tx.Start("Creating Dimensions");

                            allDimensions.Add(doc.Create.NewDimension(view, line, refArray));

                            tx.Commit();
                        }
                    }

                }

                if (allDimensions.Count > 1)
                {
                    //IList<ElementId> idsToDelete = new List<ElementId>();
                    //ReferenceArray dimensionRefsForNewDimension = new ReferenceArray();

                    //// list of element ids and geometry objects used to weed out duplicate references
                    //IList<Tuple<ElementId, GeometryObject>> geomObjList = new List<Tuple<ElementId, GeometryObject>>();

                    //Line line = null;
                    //DimensionType dimType = null;

                    //foreach (Dimension d in allDimensions)
                    //{
                    //    idsToDelete.Add(d.Id);

                    //    // take the dimension line & dimension type from the first dimension
                    //    if (line == null)
                    //    {
                    //        line = d.Curve as Line;
                    //        dimType = d.DimensionType;
                    //    }

                    //    foreach (Reference dr in d.References)
                    //    {
                    //        Element thisElement = doc.GetElement(dr);
                    //        GeometryObject thisGeomObj = thisElement.GetGeometryObjectFromReference(dr);

                    //        // do not add references to the array if the array already contains a reference
                    //        // to the same geometry element in the same element
                    //        bool duplicate = false;
                    //        foreach (Tuple<ElementId, GeometryObject> myTuple in geomObjList)
                    //        {
                    //            ElementId idInList = myTuple.Item1;
                    //            GeometryObject geomObjInList = myTuple.Item2;
                    //            if (thisElement.Id == idInList && thisGeomObj == geomObjInList)
                    //            {
                    //                duplicate = true;
                    //                break;
                    //            }
                    //        }

                    //        if (!duplicate)
                    //        {
                    //            dimensionRefsForNewDimension.Append(dr);
                    //            geomObjList.Add(new Tuple<ElementId, GeometryObject>(thisElement.Id, thisGeomObj));
                    //        }
                    //    }
                    //}

                    //List<Reference> dimensionRefs = new List<Reference>();

                    //foreach (Reference item in dimensionRefsForNewDimension)
                    //{
                    //    dimensionRefs.Add(item);
                    //}

                    //dimensionRefs = dimensionRefs.GroupBy(x => x.ElementId).Select(x => x.First()).ToList();

                    //ReferenceArray test = new ReferenceArray();

                    //foreach(Reference reference in dimensionRefs)
                    //{
                    //    test.Append(reference);
                    //}

                    //using (Transaction t = new Transaction(doc, "Dimension Consolidation"))
                    //{
                    //    t.Start();
                    //    Dimension newDim = doc.Create.NewDimension(doc.ActiveView, line, test, dimType);
                    //    doc.Delete(idsToDelete);
                    //    t.Commit();
                    //}







                    List<List<Dimension>> groupedDimensions = new List<List<Dimension>>();

                    List<List<Element>> groups = GeneralMethods.GroupElementsBy(allDimensions.Cast<Element>().ToList(), view).ToList();

                    foreach (List<Element> gro in groups)
                    {
                        groupedDimensions.Add(gro.Cast<Dimension>().ToList());
                    }


                    foreach (List<Dimension> group in groupedDimensions)
                    {
                        ReferenceArray refArray = new ReferenceArray();

                        XYZ startPoint = group.First().Curve.GetEndPoint(0);
                        XYZ endPoint = group.First().Curve.GetEndPoint(0);

                        foreach (Dimension dimension in group)
                        {
                            foreach (Reference re in dimension.References)
                            {
                                refArray.Append(re);
                            }



                        }

                        Line line = Line.CreateBound(startPoint, endPoint);

                        using (var tx = new Transaction(doc))
                        {
                            tx.Start("Creating Dimensions");

                            doc.Create.NewDimension(view, line, refArray);

                            tx.Commit();
                        }


                    }







                    //foreach (Dimension dimOrig in allDimensions)
                    //{
                    //    Curve curveOrig = dimOrig.Curve;
                    //    XYZ directionOrig = curveOrig.GetEndPoint(1).Subtract(curveOrig.GetEndPoint(0)).Normalize();
                    //    XYZ directionOrigRounded = new XYZ(Math.Round(directionOrig.X, 3), Math.Round(directionOrig.Y, 3), Math.Round(directionOrig.Z, 3));

                    //    List<Dimension> aGroupOfDimension = new List<Dimension>();

                    //    foreach (Dimension dim2 in allDimensions)
                    //    {
                    //        if (dim2 != dimOrig)
                    //        {
                    //            Curve curve = dim2.Curve;
                    //            XYZ direction = curve.GetEndPoint(1).Subtract(curve.GetEndPoint(0)).Normalize();
                    //            XYZ directionRounded = new XYZ(Math.Round(direction.X, 3), Math.Round(direction.Y, 3), Math.Round(direction.Z, 3));

                    //            if (directionOrigRounded == directionRounded)
                    //            {




                    //            }
                    //        }
                    //    }
                    //}







                }



            }
            else
            {
                TaskDialog.Show("Wrong View", "Get Your ass to a normal 2D view!!");
            }

        }



        //https://forums.autodesk.com/t5/revit-api-forum/select-elements-witch-cut-by-the-view/m-p/6793155#M20344
        public static List<Pipe> GetPipesThatCutView(UIDocument UIdoc, Element ele)
        {
            Document doc = UIdoc.Document;
            Autodesk.Revit.DB.View ActView = doc.ActiveView;

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
            List<ElementId> IdsInCutPlane = new List<ElementId>();
            foreach (Element e in pipesInCutPlane)
            {
                // "postprocessing" here if needed 
                IdsInCutPlane.Add(e.Id);
            }
            UIdoc.Selection.SetElementIds(IdsInCutPlane);

            TaskDialog.Show("yo", string.Join(", ", IdsInCutPlane));

            return pipesInCutPlane;

        }




    }
}
