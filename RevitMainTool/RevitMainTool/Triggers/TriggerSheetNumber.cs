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
using Autodesk.Revit.DB.Events;

#endregion

namespace RevitMainTool
{
    [Transaction(TransactionMode.Manual)]
    public class TriggerSheetNumber : IExternalCommand
    {

        /// <summary>
        /// Updater notifying user if an 
        /// elevation view was added.
        /// </summary>
        public class SheetWatcherUpdater : IUpdater
        {
            static AddInId _appId;
            static UpdaterId _updaterId;

            public SheetWatcherUpdater(AddInId id)
            {
                _appId = id;

                _updaterId = new UpdaterId(_appId, Global.GUIDTriggerSheetNumber);
            }
            bool IsExecuteRunning = false;
            public void Execute(UpdaterData data)
            {
                Document doc = data.GetDocument();
                Application app = doc.Application;

                foreach (ElementId id in data.GetModifiedElementIds())
                {
                    View view = doc.GetElement(id) as View;

                    if (null != view
                      && ViewType.DrawingSheet == view.ViewType)
                    {
                        if (!IsExecuteRunning)
                        {
                            IsExecuteRunning = true;

                            Parameter para = view.get_Parameter(BuiltInParameter.SHEET_APPROVED_BY);
                            para.Set(view.get_Parameter(BuiltInParameter.SHEET_NUMBER).AsValueString());


                            IsExecuteRunning = false;

                        }

                    }
                }
            }

            public string GetAdditionalInformation()
            {
                return "The Building Coder, "
                  + "http://thebuildingcoder.typepad.com";
            }

            public ChangePriority GetChangePriority()
            {
                return ChangePriority.FloorsRoofsStructuralWalls;
            }

            public UpdaterId GetUpdaterId()
            {
                return _updaterId;
            }

            public string GetUpdaterName()
            {
                return "SheetWatcherUpdater";
            }
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Application app = uiapp.Application;

            SheetWatcherUpdater updater = new SheetWatcherUpdater(app.ActiveAddInId);

            UpdaterRegistry.RegisterUpdater(updater);

            ElementCategoryFilter categoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_Sheets);

            UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), categoryFilter, Element.GetChangeTypeParameter(new ElementId((int)BuiltInParameter.SHEET_NUMBER)));
            return Result.Succeeded;
        }
    }
}
