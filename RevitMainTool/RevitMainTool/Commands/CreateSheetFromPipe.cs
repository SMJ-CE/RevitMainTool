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
using System.Windows;

#endregion

namespace RevitMainTool
{
    [Transaction(TransactionMode.Manual)]
    public class CreateSheetFromPipe : IExternalCommand
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

            if (selectedElementsIds.Count == 1)
            {
                using (var tx = new Transaction(doc))
                {
                    tx.Start("Create Drawings");

                    Element element = doc.GetElement(selectedElementsIds.First());

                    if (element is Pipe)
                    {
                        string abbreviationString = element.get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsValueString();
                        
                        if(view.GetPlacementOnSheetStatus() != ViewPlacementOnSheetStatus.CompletelyPlaced)
                        {
                            TitleBlockMethods.UpdatePaperSizeAndSMJScale(ViewMethods.CreateSheetForView(view, new string[] { "AUTO_", abbreviationString }), doc);
                        }
                        
                    }
                    else
                    {
                        TaskDialog.Show("SelectedElement", "Selected element is not a pipe. Select a element that is a pipe and try again <3");
                    }
                    tx.Commit();
                }
            }
            else
            {
                TaskDialog.Show("SelectionWrong", "No or multiple elements are selected. Please select one pipe and try again :)");
            }


            return Result.Succeeded;
        }
    }
}
