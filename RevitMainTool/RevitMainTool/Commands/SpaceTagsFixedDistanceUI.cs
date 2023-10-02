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
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Windows.Input;
using RevitMainTool.UI;


#endregion

namespace RevitMainTool
{
    [Transaction(TransactionMode.Manual)]
    public class SpaceTagsFixedDistanceUI : IExternalCommand
    {

        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            EventPlaceTagsFixedDistance handler = new EventPlaceTagsFixedDistance();
            ExternalEvent exEvent = ExternalEvent.Create(handler);

            SpaceTagsFixedDistance test = new SpaceTagsFixedDistance(commandData.Application.ActiveUIDocument, exEvent, handler);

            test.InitializeComponent();


            return Result.Succeeded;
        }
    }
}
