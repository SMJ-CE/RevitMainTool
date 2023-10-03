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
            var ele = doc.GetElement(sel.GetElementIds().First());

            using (var tx = new TransactionGroup(doc))
            {
                tx.Start("Tagging all similar");

                if (ele is Pipe pipe)
                {
                    PipeMethods.DimensionPipeToClosestGrid(pipe, uidoc);
                }

                tx.Assimilate();
            }

            return Result.Succeeded;
        }
    }
}
