#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB.Architecture;
using System.Reflection.Emit;
using RevitMainTool.Methods;
using Autodesk.Revit.DB.Plumbing;
using System.Windows.Controls;
using Autodesk.Revit.DB.Mechanical;

#endregion

namespace RevitMainTool
{
    [Transaction(TransactionMode.Manual)]
    public class Tester5 : IExternalCommand
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
            View view = doc.ActiveView;

            var selectedElementsIds = sel.GetElementIds();

            if (selectedElementsIds.Count > 0)
            {
                Level baseLevel = view.GenLevel;

                foreach ( var selectedElement in selectedElementsIds)
                {
                    if (doc.GetElement(selectedElement) is FamilyInstance instance)
                    {
                        Parameter elevationParameter = instance.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM);
                        
                        Level currentLevel = doc.GetElement(instance.LevelId) as Level;

                        if(currentLevel.Id.IntegerValue != baseLevel.Id.IntegerValue)
                        {
                            double elevationFromLevel = elevationParameter.AsDouble();

                            double elevationDifference = currentLevel.Elevation - baseLevel.Elevation;

                            double newElevationFromLevel = elevationFromLevel + elevationDifference;

                            using (var tx = new Transaction(doc))
                            {
                                tx.Start("change level");

                                instance.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM).Set(baseLevel.Id);
                                elevationParameter.Set(newElevationFromLevel);

                                tx.Commit();
                            }
                        }
                    }
                }


            }


            return Result.Succeeded;
        }
    }
}
