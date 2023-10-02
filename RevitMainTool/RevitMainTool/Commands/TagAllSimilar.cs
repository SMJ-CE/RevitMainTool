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

#endregion

namespace RevitMainTool
{
    [Transaction(TransactionMode.Manual)]
    public class TagAllSimilar : IExternalCommand
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
            var eleIds = sel.GetElementIds();

            if(eleIds.Count > 0 )
            {
                Element ele = doc.GetElement(eleIds.First());

                if( ele is IndependentTag)
                {
                    using (var tx = new TransactionGroup(doc))
                    {
                        tx.Start("Tag All Similar");

                        IndepententTagMethods.TagAllFamiliesSimilar(ele);

                        tx.Assimilate();
                    }
                }
                else if (ele is SpatialElementTag)
                {
                    TaskDialog.Show("Not a Valid Tag Dude", "Dude... That's not a valid tag. Choose one that isn't a room or space tag.");
                }
                else
                {
                    TaskDialog.Show("Not a Tag Selected", "To use this function please select a tag (that isn't a room or space tag) and then run this function");
                }
            }
            else
            {
                TaskDialog.Show("No Tags Selected", "To use this function please select a tag (that isn't a room or space tag) and then run this function");
            }

            return Result.Succeeded;
        }
    }
}
