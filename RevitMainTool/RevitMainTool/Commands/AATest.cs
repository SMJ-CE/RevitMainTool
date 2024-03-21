#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitMainTool.Methods;
using System;
using System.Linq;
using System.Windows;

#endregion

namespace RevitMainTool
{
    [Transaction(TransactionMode.Manual)]
    public class AATest : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var app = uiapp.Application;
            var doc = uidoc.Document;

            var sel = uidoc.Selection;
            View GraphicalView = uidoc.ActiveGraphicalView;
            XYZ locationOnSheet = null;

            var eleIds = sel.GetElementIds();

            if (eleIds.Count > 0)
            {
                using (var tx = new TransactionGroup(doc))
                {
                    tx.Start("Make Grids and crop the same and place the same on sheet");

                    if (GraphicalView is ViewSheet grahpSheet)
                    {
                        Viewport GraphicalViewport = ViewMethods.GetMainPlanViewInSheet(grahpSheet);
                        locationOnSheet = GraphicalViewport.GetLocationOnSheet();
                        GraphicalView = doc.GetElement(GraphicalViewport.ViewId) as View;
                    }

                    if(GraphicalView != null)
                    {
                        var catFilter = new ElementCategoryFilter(BuiltInCategory.OST_Grids);
                        var allGridsInView = new FilteredElementCollector(doc, GraphicalView.Id).WhereElementIsNotElementType().WherePasses(catFilter);
                        bool cropViewActive = GraphicalView.CropBoxActive;
                        CurveLoop curvesOfCrop = null;
                        bool cropVisible = false;
                        bool annotationCrop = false;

                        if (cropViewActive)
                        {
                            if (!GraphicalView.GetCropRegionShapeManager().Split)
                            {
                                curvesOfCrop = GraphicalView.GetCropRegionShapeManager().GetCropShape().First();
                                cropVisible = GraphicalView.CropBoxVisible;
                                //annotationCrop = GraphicalView.GetCropRegionShapeManager();
                            }
                        }

                        foreach (var eleId in eleIds)
                        {
                            Element ele = doc.GetElement(eleId);
                            View eleView = null;

                            if (ele is ViewSheet viewSheet)
                            {

                                Viewport viewTest = ViewMethods.GetMainPlanViewInSheet(viewSheet);

                                if(viewTest != null)
                                {
                                    bool isViewPinned = viewTest.Pinned;

                                    if (isViewPinned)
                                    {
                                        using (var tra = new Transaction(doc))
                                        {
                                            tra.Start("Unpin Viewport");

                                            viewTest.Pinned = false;

                                            tra.Commit();
                                        }
                                    }

                                    eleView = doc.GetElement(viewTest.ViewId) as View;

                                    viewTest.SetLocationOnSheet(locationOnSheet);

                                    using (var tra = new Transaction(doc))
                                    {
                                        tra.Start("Pin Viewport");

                                        viewTest.Pinned = true;

                                        tra.Commit();
                                    }
                                }
                            }
                            else if (ele is View view)
                            {
                                eleView = view;
                            }

                            if (eleView != null)
                            {
                                if (curvesOfCrop != null)
                                {
                                    using (var tra = new Transaction(doc))
                                    {
                                        tra.Start("Make Crop the Same");

                                        eleView.CropBoxActive = true;
                                        eleView.GetCropRegionShapeManager().SetCropShape(curvesOfCrop);
                                        eleView.CropBoxVisible = cropVisible;

                                        tra.Commit();
                                    }
                                }

                                foreach (Element gridElement in allGridsInView)
                                {
                                    if (gridElement is Grid grid)
                                    {
                                        using (var tra = new Transaction(doc))
                                        {
                                            tra.Start("Make grids 2D");

                                            grid.SetDatumExtentType(DatumEnds.End0, eleView, DatumExtentType.ViewSpecific);
                                            grid.SetDatumExtentType(DatumEnds.End1, eleView, DatumExtentType.ViewSpecific);

                                            tra.Commit();
                                        }

                                        var test2 = grid.GetCurvesInView(DatumExtentType.ViewSpecific, GraphicalView);
                                        Curve newCurve = grid.GetCurvesInView(DatumExtentType.ViewSpecific, GraphicalView).First();

                                        var origCurve = grid.GetCurvesInView(DatumExtentType.ViewSpecific, eleView).First() as Line;
                                        var origDirection = origCurve.Direction;

                                        var newCurveStartPoint = newCurve.GetEndPoint(0);
                                        var newCurveStartHelperPoint = origDirection.ConvertToUV().RotateVector(90).Add(newCurveStartPoint.ConvertToUV());

                                        var newCurveEndPoint = newCurve.GetEndPoint(1);
                                        var newCurveEndHelperPoint = origDirection.ConvertToUV().RotateVector(90).Add(newCurveEndPoint.ConvertToUV());

                                        var origCurveStartPoint = origCurve.GetEndPoint(0);
                                        var origCurveEndPoint = origCurve.GetEndPoint(1);

                                        UV intersectionStart = XYZMethods.IntersectionOfTwoLines(origCurveStartPoint.ConvertToUV(), origCurveEndPoint.ConvertToUV(), newCurveStartPoint.ConvertToUV(), newCurveStartHelperPoint);
                                        UV intersectionEnd = XYZMethods.IntersectionOfTwoLines(origCurveStartPoint.ConvertToUV(), origCurveEndPoint.ConvertToUV(), newCurveEndPoint.ConvertToUV(), newCurveEndHelperPoint);

                                        using (var tra = new Transaction(doc))
                                        {
                                            tra.Start("Make Grids the same");
                                            var theNewLine = Line.CreateBound(intersectionStart.ConvertToXYZ(origCurveStartPoint.Z), intersectionEnd.ConvertToXYZ(origCurveEndPoint.Z));

                                            grid.SetCurveInView(DatumExtentType.ViewSpecific, eleView, theNewLine);
                                            tra.Commit();
                                        }


                                    }
                                }
                            }
                        }
                    }
                    tx.Assimilate();
                }


            }


            return Result.Succeeded;
        }

        bool doing(XYZ point1, XYZ point2, XYZ testPoint)
        {
            var m = (point2.Y - point1.Y) / (point2.X - point1.X);
            var b = point1.Y - (m * point1.X);


            return m * testPoint.X + b == testPoint.Y;
        }


        void AlignOffAxisGrid(Grid grid)
        {
            //Grid grid = doc.GetElement( 
            //  sel.GetElementIds().FirstOrDefault() ) as Grid;

            Document doc = grid.Document;

            XYZ direction = grid.Curve
              .GetEndPoint(1)
              .Subtract(grid.Curve.GetEndPoint(0))
              .Normalize();

            double distance2hor = direction.DotProduct(XYZ.BasisY);
            double distance2vert = direction.DotProduct(XYZ.BasisX);
            double angle = 0;

            // Maybe use another criterium then <0.0001

            double max_distance = 0.0001;

            if (Math.Abs(distance2hor) < max_distance)
            {
                XYZ vector = direction.X < 0
                  ? direction.Negate()
                  : direction;

                angle = Math.Asin(-vector.Y);
            }

            if (Math.Abs(distance2vert) < max_distance)
            {
                XYZ vector = direction.Y < 0
                  ? direction.Negate()
                  : direction;

                angle = Math.Asin(vector.X);
            }

            if (angle.CompareTo(0) != 0)
            {
                ElementTransformUtils.RotateElement(doc,
                      grid.Id,
                      Line.CreateBound(grid.Curve.GetEndPoint(0),
                        grid.Curve.GetEndPoint(0).Add(XYZ.BasisZ)),
                      angle);
            }
        }
    }
}
