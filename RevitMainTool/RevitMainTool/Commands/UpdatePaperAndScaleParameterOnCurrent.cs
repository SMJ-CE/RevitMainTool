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
using RevitMainTool.Models;
using System.Collections.ObjectModel;

#endregion

namespace RevitMainTool
{
    [Transaction(TransactionMode.Manual)]
    public class UpdatePaperAndScaleParameterOnCurrent : IExternalCommand
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

            ViewSheet currentSheet = null;

            if (view is ViewSheet)
            {
                currentSheet = (ViewSheet)view;
            }
            else
            {
                string currentSheetNumber = view.get_Parameter(BuiltInParameter.VIEWPORT_SHEET_NUMBER).AsValueString();

                currentSheet = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Sheets).First(x => (x as ViewSheet).SheetNumber == currentSheetNumber) as ViewSheet;
            }

            if (currentSheet != null)
            {
                using (var tx = new Transaction(doc))
                {
                    tx.Start("Updating SMJ Scale and Paper Sizes");

                    TitleBlockMethods.UpdatePaperSizeAndSMJScale(currentSheet, doc);

                    tx.Commit();
                }
            }

            return Result.Succeeded;
        }
    }
}
