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
    public class AAMoreTesty : IExternalCommand
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

            CurveLoop savedBox = GraphicalView.GetCropRegionShapeManager().GetCropShape().First();
            var isNotRectangular = GraphicalView.GetCropRegionShapeManager().ShapeSet;

            if (isNotRectangular)
            {
                using (var tx = new Transaction(doc))
                {
                    tx.Start("Reset Crop Shape");

                    GraphicalView.GetCropRegionShapeManager().RemoveCropRegionShape();

                    tx.Commit();
                }
            }

            bool cropBoxActive = GraphicalView.CropBoxActive;
            bool cropBoxVisible = GraphicalView.CropBoxVisible;

            XYZ start = new XYZ(-9999999, -9999999, 0);
            XYZ End = new XYZ(9999999, 9999999, 0);
            BoundingBoxXYZ bigBoundingBox = new BoundingBoxXYZ();
            bigBoundingBox.Min = start;
            bigBoundingBox.Max = End;


            using (var tx = new Transaction(doc))
            {
                tx.Start("Make Crop Big");

                GraphicalView.CropBoxActive = true;
                GraphicalView.CropBoxVisible = true;
                GraphicalView.CropBox = bigBoundingBox;

                tx.Commit();
            }

            using (var tx = new Transaction(doc))
            {
                tx.Start("SetLocation");

                GraphicalView.CropBoxActive = cropBoxActive;
                GraphicalView.CropBoxVisible = cropBoxVisible;
                GraphicalView.GetCropRegionShapeManager().SetCropShape(savedBox);

                tx.Commit();
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
