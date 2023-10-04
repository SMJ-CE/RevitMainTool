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

#endregion

namespace RevitMainTool
{
    [Transaction(TransactionMode.Manual)]
    public class Tester : IExternalCommand
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
            var ele1 = doc.GetElement(sel.GetElementIds().ToList()[0]);
            //var ele2 = doc.GetElement(sel.GetElementIds().ToList()[1]);

            View view = doc.ActiveView;

            //TaskDialog.Show("theTest", GeneralMethods.DoBoundingBoxesOverlap(ele1 , ele2, doc.ActiveView).ToString());

            //List<Element> list = new List<Element>();

            //foreach(ElementId id in sel.GetElementIds())
            //{
            //    list.Add(doc.GetElement(id));
            //}

            //TaskDialog.Show("Yo", PipeMethods.DoDimensionsHaveSameDirection(ele1 as Dimension, ele2 as Dimension).ToString());



            //List<List<Element>> groupedElements = GeneralMethods.GroupElementsByBoundingBox(list, view);

            //List<List<string>> stringGroup = new List<List<string>>();

            //foreach (List<Element> ele in groupedElements)
            //{
            //    List<string> strings = new List<string>();
            //    foreach (Element ele2 in ele)
            //    {
            //        strings.Add(ele2.Name);
            //    }

            //    stringGroup.Add(strings);

            //}

            //TaskDialog.Show("bro", string.Join(", ", stringGroup));

            using (var tx = new TransactionGroup(doc))
            {
                tx.Start("Tagging all similar");

                if (ele1 is Pipe pipe)
                {
                    PipeMethods.DimensionPipeToClosestGrid(pipe, uidoc);
                }

                tx.Assimilate();
            }

            return Result.Succeeded;
        }
    }
}
