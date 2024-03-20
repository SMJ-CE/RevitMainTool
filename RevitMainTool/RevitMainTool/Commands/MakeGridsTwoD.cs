#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitMainTool.Methods;
using System;
using System.Linq;

#endregion

namespace RevitMainTool
{
    [Transaction(TransactionMode.Manual)]
    public class MakeGridsTwoD : IExternalCommand
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

            if (GraphicalView != null)
            {
                var catFilter = new ElementCategoryFilter(BuiltInCategory.OST_Grids);
                var allGridsInView = new FilteredElementCollector(doc, GraphicalView.Id).WhereElementIsNotElementType().WherePasses(catFilter);
                using (var txg = new TransactionGroup(doc))
                {
                    txg.Start("Make grids 2D");

                    foreach (Element gridElement in allGridsInView)
                    {
                        if (gridElement is Grid grid)
                        {
                            using (var tra = new Transaction(doc))
                            {
                                tra.Start("Make grids 2D");

                                grid.SetDatumExtentType(DatumEnds.End0, GraphicalView, DatumExtentType.ViewSpecific);
                                grid.SetDatumExtentType(DatumEnds.End1, GraphicalView, DatumExtentType.ViewSpecific);

                                tra.Commit();
                            }
                        }
                    }

                    txg.Assimilate();
                }
                
            }

            return Result.Succeeded;
        }
    }
}
