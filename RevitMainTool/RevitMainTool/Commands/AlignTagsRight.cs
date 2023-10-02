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
    public class AlignTagsRight : IExternalCommand
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

            if (eleIds.Count == 0)
            {
                TaskDialog.Show("No Selection", "Nothing is selected. Please select multiple tags.");
            }
            else
            {
                List<IndependentTag> tags = eleIds
                    .Select(doc.GetElement)
                    .OfType<IndependentTag>()
                    .ToList();

                if (tags.Count <= 1)
                {
                    TaskDialog.Show("No Selection", "There is one tag selected. Please select multiple tags.");
                }
                else
                {
                    using (var trGr = new TransactionGroup(doc))
                    {
                        trGr.Start("Aligning tags to the Right");
                        IndepententTagMethods.AlignTagsRight(tags);

                        ICollection<ElementId> newSelection = new HashSet<ElementId>(tags.Select(tag => tag.Id));
                        sel.SetElementIds(newSelection);

                        trGr.Assimilate();
                    }
                }
            }

            return Result.Succeeded;
        }
    }
}
