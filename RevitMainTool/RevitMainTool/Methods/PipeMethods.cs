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
        public static void DimensionPipeToClosestGrid(Pipe pipe)
        {
            Document doc = pipe.Document;
            View view = doc.ActiveView;

            if (view is ViewPlan)
            {




            }




        }




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
