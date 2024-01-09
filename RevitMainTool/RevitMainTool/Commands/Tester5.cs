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
    public class Tester5 : IExternalCommand
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

            if (selectedElementsIds.Count > 0)
            {



                foreach ( var selectedElement in selectedElementsIds)
                {
                    var currentElement = doc.GetElement(selectedElement);

                    if (currentElement is ViewSheet currentViewSheet)
                    {
                        var allViewsOnSheet = currentViewSheet.GetAllPlacedViews();

                        if(allViewsOnSheet.Count > 0)
                        {
                            foreach(var viewId in allViewsOnSheet)
                            {
                                var currentView = doc.GetElement(viewId) as View;

                                if(currentView.ViewType == ViewType.FloorPlan)
                                {

                                }

                            }
                        }
                        

                    }


                }


            }


            return Result.Succeeded;
        }
    }
}
