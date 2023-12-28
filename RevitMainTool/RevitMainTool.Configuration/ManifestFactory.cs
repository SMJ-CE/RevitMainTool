using Autodesk.RevitAddIns;
using System;
using System.IO;

namespace RevitMainTool.Configuration
{
    public class ManifestFactory
    {
        public void Create(string directory)
        {
            //create a new addin manifest
            RevitAddInManifest manifest = new RevitAddInManifest();

            //create an external application
            RevitAddInApplication application = new RevitAddInApplication(
                "RevitMainTool",
                $"{directory}\\RevitMainTool.dll",
               new Guid("608681DE-D20E-4CAC-B706-9756A59A4B7F"),
                "RevitMainTool.App",
                "SMJ Addin Creator");

            manifest.AddInApplications.Add(application);

            //save manifest to a file
            RevitProduct revitProduct = RevitProductUtility.GetAllInstalledRevitProducts()[0];
            var targetDir = Path.Combine(revitProduct.CurrentUserAddInFolder, "RevitMainTool.addin");
            manifest.SaveAs(targetDir);
        }
    }
}
