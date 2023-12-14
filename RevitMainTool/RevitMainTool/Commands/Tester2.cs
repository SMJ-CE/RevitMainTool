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

#endregion

namespace RevitMainTool
{
    [Transaction(TransactionMode.Manual)]
    public class Tester2 : IExternalCommand
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
                if (selectedElementsIds.Count == 1) 
                {
                    Element selectedElement = doc.GetElement(selectedElementsIds.First());

                    if (selectedElement is RevitLinkInstance revitLink)
                    {
                        var rooms = new FilteredElementCollector(revitLink.GetLinkDocument()).OfCategory(BuiltInCategory.OST_Rooms).Cast<Room>();

                        XYZ linkOffsetVector = revitLink.GetTransform().Origin;

                        using (var tx = new Transaction(doc))
                        {
                            tx.Start("Hiding grids");

                            foreach (var room in rooms)
                            {
                                //string number = room.LookupParameter("Number").AsValueString();

                                //if (number == "R07.714")
                                //{

                                //}
                                SpaceMethods.CreateOrUpdateSpaceFromRoomInLinkedFile(doc, room, linkOffsetVector);
                            }

                            tx.Commit();
                        }

                        
                    }
                }
            }

            return Result.Succeeded;
        }
    }
}
