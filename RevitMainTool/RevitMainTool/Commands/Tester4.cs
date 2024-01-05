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
    public class Tester4 : IExternalCommand
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

            var allViewSheets = new FilteredElementCollector(doc).OfClass(typeof(ViewSheet));


            if (allViewSheets.Count() > 0)
            {
                using (var tx = new Transaction(doc))
                {
                    tx.Start("Updating SMJ Scale and Paper Sizes");

                    foreach (ViewSheet sheet in allViewSheets)
                    {
                        var viewElementIds = sheet.GetAllPlacedViews();

                        if(viewElementIds.Count() > 0)
                        {
                            HashSet<string> scalesInViewSheet = new HashSet<string>();

                            foreach (var viewElement in viewElementIds)
                            {
                                var viewOnSheetElement = doc.GetElement(viewElement);

                                if (viewOnSheetElement is View viewOnSheet)
                                {
                                    if (viewOnSheet.ViewType != ViewType.Legend && viewOnSheet.ViewType != ViewType.ThreeD && viewOnSheet.ViewType != ViewType.DraftingView)
                                    {
                                        scalesInViewSheet.Add("1:" + viewOnSheet.Scale.ToString());
                                    }
                                }
                            }

                            sheet.LookupParameter("SMJ Scale").Set(string.Join(", ", scalesInViewSheet));
                        }
                        
                        var titleBlocks = new FilteredElementCollector(doc, sheet.Id).OfCategory(BuiltInCategory.OST_TitleBlocks);

                        FamilyInstance choosenOne = null;
                        double temporaryDouble = 0;

                        foreach (FamilyInstance titleBlock in titleBlocks)
                        {
                            var param = titleBlock.get_Parameter(BuiltInParameter.SHEET_WIDTH);
                            double width = param.AsDouble();

                            if(width > temporaryDouble)
                            {
                                temporaryDouble = width;
                                choosenOne = titleBlock;
                            }
                        }

                        if(choosenOne != null)
                        {
                            string tes = choosenOne.Symbol.Name;

                            sheet.LookupParameter("Paper Size").Set(tes.Remove(tes.Length - 1, 1));
                        }
                    }

                    tx.Commit();
                }
            }

            return Result.Succeeded;
        }
    }
}
