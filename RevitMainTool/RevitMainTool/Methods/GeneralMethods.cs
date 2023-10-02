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

namespace RevitMainTool
{
    public static class GeneralMethods
    {
        public static void GetClosestGridLines(Element element)
        {
            Document doc = element.Document;
            View view = doc.ActiveView;

            var grids = new FilteredElementCollector(doc, view.Id).OfCategory(BuiltInCategory.OST_Grids).Cast<Grid>();

            List<Grid> horizontalGrids = grids.Where(x => (x.Curve as Line).Direction.X == 1 || (x.Curve as Line).Direction.X == -1).ToList();
            List<Grid> verticalGrids = grids.Where(x => (x.Curve as Line).Direction.Y == 1 || (x.Curve as Line).Direction.Y == -1).ToList();

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
                return;
            }





            
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

        //public static BoundingBoxXYZ GetMaxXYZ(List<Element> elements)
        //{



        //}



    }
}
