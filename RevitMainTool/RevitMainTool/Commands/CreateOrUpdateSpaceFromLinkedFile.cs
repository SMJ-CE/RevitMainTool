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
using RevitMainTool.SelectionFilters;

#endregion

namespace RevitMainTool
{
    [Transaction(TransactionMode.Manual)]
    public class CreateOrUpdateSpaceFromLinkedFile : IExternalCommand
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
                        SpaceMethods.CreateSpaceFromRoomInLinkedFile(doc, revitLink);
                    }
                    else
                    {
                        TaskDialog.Show("Selected Element is Not Revit Link", "Selected element is not a Revit link, select one Revit link and try again");
                    }
                }
                else
                {
                    TaskDialog.Show("Multiple Elements Selected", "Multiple elements selected, select one Revit link and try again");
                }
            }
            else
            {
                Reference selectedLink;
                try 
                {
                    selectedLink = sel.PickObject(ObjectType.Element, new SelectionFilterLinkedDocument(doc));
                } 
                catch
                {
                    return Result.Cancelled;
                }

                if(selectedLink == null)
                {
                    TaskDialog.Show("No Link Selected", "There is no link selected, select a link and try again\nif it so happens that you see this message then you have done the impossible. Congratulations!");
                }
                else
                {
                    IList<Reference> selectedRooms;
                    RevitLinkInstance revitLinkInstance = doc.GetElement(selectedLink.ElementId) as RevitLinkInstance;
                    Document SelectedLinkDocument = revitLinkInstance.GetLinkDocument();

                    try
                    {
                        //TaskDialog.Show("SelectedLink", SelectedLinkDocument.Title);
                        selectedRooms = sel.PickObjects(ObjectType.LinkedElement, new SelectionFilterRoomsInLinkedDocument(doc, SelectedLinkDocument));
                    }
                    catch
                    {
                        return Result.Cancelled;
                    }

                    if (selectedRooms.Count == 0)
                    {
                        TaskDialog.Show("No Rooms Selected", "There are no rooms selected, select a or many rooms and try again");
                    }
                    else
                    {
                        List<Room> rooms = new List<Room>();
                        foreach (Reference selectedReference in selectedRooms)
                        {
                            if(SelectedLinkDocument.GetElement(selectedReference.LinkedElementId) is Room room)
                            {
                                rooms.Add(room);
                            }
                        }

                        if (rooms.Count != 0)
                        {
                            XYZ linkOffsetVector = revitLinkInstance.GetTransform().Origin;
                            SpaceMethods.CreateSpaceFromRoomInLinkedFile(doc, rooms, linkOffsetVector);
                        }
                        
                        //TaskDialog.Show("SelectedLink", doc.GetElement(selectedLink.ElementId).Name);
                        //TaskDialog.Show("selectedRooms", (doc.GetElement(selectedLink.ElementId) as RevitLinkInstance).GetLinkDocument().GetElement(selectedRooms.First().LinkedElementId).Name);
                    }
                }



            }

            return Result.Succeeded;
        }
    }
}
