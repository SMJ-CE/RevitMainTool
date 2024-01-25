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
using System.Collections.ObjectModel;

#endregion

namespace RevitMainTool
{
    [Transaction(TransactionMode.Manual)]
    public class ElementUnhideEverythingInView : IExternalCommand
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

            using (var tx = new Transaction(doc))
            {
                tx.Start("Unhide All Hidden");

                view.EnableRevealHiddenMode();

                var elementsInViewWithHidden = new FilteredElementCollector(doc, view.Id);

                ICollection<ElementId> elementIdsToBeUnhidden = new Collection<ElementId>();

                foreach (var element in elementsInViewWithHidden)
                {
                    if (element.CanBeHidden(view) && element.IsHidden(view))
                    {
                        elementIdsToBeUnhidden.Add(element.Id);
                    }
                }

                view.UnhideElements(elementIdsToBeUnhidden);

                view.DisableTemporaryViewMode(TemporaryViewMode.RevealHiddenElements);


                tx.Commit();
            }


            

            return Result.Succeeded;
        }
    }
}
