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
    public class DimensionPipesThatCutView : IExternalCommand
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
            var eleId = sel.GetElementIds();
            
            if (eleId.Count == 1)
            {
                if (doc.GetElement(eleId.First()) is Pipe pipe)
                {
                    using (var tx = new TransactionGroup(doc))
                    {
                        tx.Start("Dimensioning Pipes");

                        PipeMethods.DimensionPipeToClosestGrid(pipe, uidoc);

                        tx.Assimilate();
                    }
                }
            }
            else if (eleId.Count > 1)
            {
                TaskDialog.Show("Too many selected", "Please select one element at a time. Element needs to be a pipe for it to work");
            }
            else
            {
                TaskDialog.Show("No Selection", "Please select a pipe");
            }
            
            

            return Result.Succeeded;
        }
    }
}
