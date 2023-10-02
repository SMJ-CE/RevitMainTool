#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.IO;

#endregion

namespace RevitMainTool
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            // Create a custom ribbon tab
            var tabName = "RevitMainTool";
            application.CreateRibbonTab(tabName);

            // Add a new ribbon panel
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "RevitMainTool Tab");

            new ButtonBuilder("Export IFC", typeof(Command))
                .Text("RevitMainTool")
                .Build(panel);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

    }
}
