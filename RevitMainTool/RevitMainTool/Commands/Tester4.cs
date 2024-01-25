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
using Ookii.Dialogs.Wpf;
using Microsoft.Win32;
using System.IO;
using BIM.IFC.Export.UI;
using static Autodesk.Revit.DB.SpecTypeId;

#endregion

namespace RevitMainTool
{
    [Transaction(TransactionMode.Manual)]
    public class Tester4 : IExternalCommand
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

            var saveFileDialog = new SaveFileDialog();
            if(saveFileDialog.ShowDialog() == true)
            {
                //Autodesk.Revit.UI.TaskDialog mainDialog = new Autodesk.Revit.UI.TaskDialog("Hello, Revit!");
                //mainDialog.MainInstruction = "Hello, Revit!";
                //mainDialog.MainContent = saveFileDialog.FileName;

                //mainDialog.Show();

                string folderPath = Path.GetDirectoryName(saveFileDialog.FileName);
                string fileName = Path.GetFileName(saveFileDialog.FileName);

                using (var tg = new TransactionGroup(doc))
                {
                    tg.Start("Exporting to IFC");

                    using (var tx = new Transaction(doc))
                    {
                        tx.Start("binding Links");

                        var allLinkedFiles = new FilteredElementCollector(doc, view.Id).OfClass(typeof(RevitLinkInstance));

                        foreach (RevitLinkInstance instance in allLinkedFiles)
                        {
                            


                        }


                        tx.Commit();
                    }

                    IFCExportOptions exportOptions = new IFCExportOptions();

                    exportOptions.FileVersion = IFCVersion.IFC2x3;

                    BIM.IFC.Export.UI.IFCExportConfiguration myIFCExportConfiguration = BIM.IFC.Export.UI.IFCExportConfiguration.CreateDefaultConfiguration();

                    //myIFCExportConfiguration.ExportLinkedFiles = true;
                    //myIFCExportConfiguration.UseActiveViewGeometry = true;
                    //myIFCExportConfiguration.VisibleElementsOfCurrentView = true;

                    myIFCExportConfiguration.UpdateOptions(exportOptions, view.Id);

                    using (var tx = new Transaction(doc))
                    {
                        tx.Start("Exporting to IFC");

                        

                        doc.Export(folderPath, fileName, exportOptions);

                        tx.Commit();
                    }

                    tg.RollBack();
                }

                

                



            }


            return Result.Succeeded;
        }
    }
}
