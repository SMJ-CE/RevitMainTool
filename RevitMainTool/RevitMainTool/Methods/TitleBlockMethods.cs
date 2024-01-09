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
                var viewElementIds = sheet.GetAllPlacedViews();

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

                    sheet.LookupParameter("SMJ Scale").Set(string.Join(", ", scalesInViewSheet));
                }

                var sheetId = sheet.Id;

                var titleBlocksInView = titleBlocksInDoc.Where(x => x.OwnerViewId == sheetId);

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
                    sheet.LookupParameter("Paper Size").Set(GetPaperSize(choosenOne));
                }
            }
        }


    }
}
