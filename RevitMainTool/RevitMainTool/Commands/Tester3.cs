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
    public class Tester3 : IExternalCommand
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

            var allPipes = new FilteredElementCollector(doc).OfClass(typeof(Pipe)).Cast<Pipe>();

            var allDucts = new FilteredElementCollector(doc).OfClass(typeof(Duct)).Cast<Duct>();

            var randoPipe = allPipes.First();

            string parameterName = "IsVerticale";

            if (randoPipe != null)
            {
                var param = randoPipe.LookupParameter(parameterName);

                if (param != null)
                {
                    using (var tx = new Transaction(doc))
                    {
                        tx.Start("Setting IsVerticale Parameter");

                        foreach (var pipe in allPipes)
                        {

                            var pipeParam = pipe.LookupParameter(parameterName);

                            if (pipeParam != null && !pipeParam.IsReadOnly)
                            {
                                Line pipeLine = (pipe.Location as LocationCurve).Curve as Line;

                                XYZ pipeDirection = pipeLine.Direction;

                                double dirX = Math.Round(pipeDirection.X, 4);
                                double dirY = Math.Round(pipeDirection.Y, 4);

                                bool isNotVerticale = dirX != 0 || dirY != 0;

                                int setValue = isNotVerticale ? 0 : 1;

                                var pipeValue = pipeParam.AsInteger();

                                if( pipeValue != setValue)
                                {
                                    pipeParam.Set(setValue);
                                }
                            }
                        }

                        foreach (var duct in allDucts)
                        {

                            var ductParam = duct.LookupParameter(parameterName);

                            if (ductParam != null && !ductParam.IsReadOnly)
                            {
                                Line ductLine = (duct.Location as LocationCurve).Curve as Line;

                                XYZ ductDirection = ductLine.Direction;

                                double dirX = Math.Round(ductDirection.X, 4);
                                double dirY = Math.Round(ductDirection.Y, 4);

                                bool isNotVerticale = dirX != 0 || dirY != 0;

                                int setValue = isNotVerticale ? 0 : 1;

                                var ductValue = ductParam.AsInteger();

                                if (ductValue != setValue)
                                {
                                    ductParam.Set(setValue);
                                }
                            }
                        }

                        tx.Commit();
                    }
                }
            }

            return Result.Succeeded;
        }
    }
}
