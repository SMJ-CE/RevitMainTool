using Autodesk.Revit.DB;
using RevitMainTool.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RevitMainTool.Methods
{
    public class TitleBlockMethods
    {
        public static string GetPaperSize(Element choosenOne)
        {
            if(choosenOne.Category.Name == "Title Blocks")
            {
                double titleBlockWidth = choosenOne.get_Parameter(BuiltInParameter.SHEET_WIDTH).AsDouble() * 304.8;
                double titleBlockHeight = choosenOne.get_Parameter(BuiltInParameter.SHEET_HEIGHT).AsDouble() * 304.8;

                string input = null;

                foreach (PaperSizes paperSize in Global.PaperSizes)
                {
                    if (paperSize.IsAMatch(titleBlockHeight, titleBlockWidth, 1))
                    {
                        input = paperSize.Name;
                        break;
                    }
                }

                if (input == null)
                {
                    double firstNumber = titleBlockWidth < titleBlockHeight ? titleBlockWidth : titleBlockHeight;
                    double secondNumber = titleBlockWidth < titleBlockHeight ? titleBlockHeight : titleBlockWidth;
                    input = firstNumber + "x" + secondNumber;
                }

                return input;
            }
            return null;
        }


        public static void UpdatePaperSizeAndSMJScale(IEnumerable<ViewSheet> viewSheetsToBeUpdated, Document doc)
        {
            var titleBlocksInDoc = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).Where(x => x.Category.Name == "Title Blocks");

            foreach (ViewSheet sheet in viewSheetsToBeUpdated)
            {
                UpdatePaperSizeAndSMJScale(sheet, doc, titleBlocksInDoc);
            }
        }

        public static void UpdatePaperSizeAndSMJScale(ViewSheet sheet, Document doc)
        {
            var titleBlocksInDoc = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).Where(x => x.Category.Name == "Title Blocks");

            UpdatePaperSizeAndSMJScale(sheet, doc, titleBlocksInDoc);
        }

        public static void UpdatePaperSizeAndSMJScale(ViewSheet sheet, Document doc, IEnumerable<Element> titleBlocks)
        {
            var viewElementIds = sheet.GetAllPlacedViews();
            Parameter SMJScaleParameter = sheet.LookupParameter("SMJ Scale");


            //Join multiple scales together
            if (SMJScaleParameter != null)
            {
                if (viewElementIds.Count() > 0)
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

                    SMJScaleParameter.Set(string.Join(", ", scalesInViewSheet));
                }
            }

            //Take only the largest scale
            //if (SMJScaleParameter != null)
            //{
            //    if (viewElementIds.Count() > 0)
            //    {
            //        List<int> scalesInViewSheet = new List<int>();

            //        foreach (var viewElement in viewElementIds)
            //        {
            //            var viewOnSheetElement = doc.GetElement(viewElement);

            //            if (viewOnSheetElement is View viewOnSheet)
            //            {
            //                if (viewOnSheet.ViewType != ViewType.Legend && viewOnSheet.ViewType != ViewType.ThreeD && viewOnSheet.ViewType != ViewType.DraftingView)
            //                {
            //                    scalesInViewSheet.Add(viewOnSheet.Scale);
            //                }
            //            }
            //        }

            //        if (scalesInViewSheet.Count() > 0)
            //        {
            //            SMJScaleParameter.Set("1:" + scalesInViewSheet.Max().ToString());
            //        }

            //    }
            //}


            Parameter paperSizeParameter = sheet.LookupParameter("Paper Size");

            if (paperSizeParameter != null)
            {
                var sheetId = sheet.Id;
                var titleBlocksInView = titleBlocks.Where(x => x.OwnerViewId == sheetId);
                FamilyInstance choosenOne = null;
                double temporaryDouble = 0;

                foreach (FamilyInstance titleBlock in titleBlocksInView)
                {
                    var param = titleBlock.get_Parameter(BuiltInParameter.SHEET_WIDTH);
                    double width = param.AsDouble();

                    if (width > temporaryDouble)
                    {
                        temporaryDouble = width;
                        choosenOne = titleBlock;
                    }
                }

                if (choosenOne != null)
                {
                    paperSizeParameter.Set(GetPaperSize(choosenOne));
                }
            }
        }

    }
}
