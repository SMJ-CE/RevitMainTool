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

#endregion

namespace RevitMainTool
{
    [Transaction(TransactionMode.Manual)]
    public class Tester : IExternalCommand
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


            var links = new FilteredElementCollector(doc, view.Id).WhereElementIsNotElementType().OfCategory(BuiltInCategory.OST_RvtLinks).ToList();

            foreach (var link in links)
            {
                Document linkDoc = link.Document;

                var grids = new FilteredElementCollector(linkDoc, view.Id).OfCategory(BuiltInCategory.OST_Grids).Select(x => x.Id).ToList();

                if (grids.Count > 0)
                {
                    using (var tx = new Transaction(doc))
                    {
                        tx.Start("Hiding grids");

                        view.HideElements(grids);

                        tx.Commit();
                    }
                }
            }

            return Result.Succeeded;
        }
    }
}
