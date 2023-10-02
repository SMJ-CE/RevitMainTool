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
    public class SpaceTagsEvenly : IExternalCommand
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

            if (eleIds.Count != 0)
            {
                List<IndependentTag> tags = new List<IndependentTag>();

                foreach (var eleid in eleIds)
                {
                    Element ele = doc.GetElement(eleid);
                    if (ele != null && ele is IndependentTag)
                    {
                        tags.Add(ele as IndependentTag);
                    }
                }

                if (tags.Count > 1)
                {
                    using (var trGr = new TransactionGroup(doc))
                    {
                        trGr.Start("Space Tags Evenly");

                        IndepententTagMethods.SpaceTagsEvenly(tags, XOrY.SumOfXandY);

                        ICollection<ElementId> newSelection = new HashSet<ElementId>();

                        foreach (var tag in tags)
                        {
                            newSelection.Add(tag.Id);
                        }

                        sel.SetElementIds(newSelection);


                        trGr.Assimilate();
                    }

                }
                else
                {
                    TaskDialog dia = new TaskDialog("No Selection");
                    dia.MainContent = "There is one or no Tags selected. Please select Multiple Tags";
                    dia.Show();
                }

            }
            else
            {
                TaskDialog dia = new TaskDialog("No Selection");
                dia.MainContent = "Nothing is selected. Please select Multiple Tags";
                dia.Show();
            }


            return Result.Succeeded;
        }
    }
}
