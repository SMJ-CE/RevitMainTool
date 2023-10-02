using Autodesk.RevitAddIns;
using System;
using System.IO;
using System.Linq;

namespace RevitMainTool.Configuration
{
    internal class ManifestFactory
    {
        public void Create(string directory)
        {
            //create a new addin manifest
            RevitAddInManifest manifest = new RevitAddInManifest();

            var dllRef = Path.Combine(directory, "RevitMainTool.dll");
            //create an external application
            RevitAddInApplication application = new RevitAddInApplication(
                "RevitMainTool",
                dllRef,
               new Guid("4E48ABF3-4BAE-490F-824F-FF55232B3342"),
                "RevitMainTool.App",
                "Eivind Vørmadal");

            manifest.AddInApplications.Add(application);
            var products = RevitProductUtility.GetAllInstalledRevitProducts();

            if (!products.Any())
            {
                throw new Exception("Revit is not installed");
            }

            //TODO find a way to handle multiple versions of Revit
            var product = products.Where(x => (int)x.Version == 13).FirstOrDefault();

            if (product == null)
            {
                throw new Exception("Version 2024 is not installed");
            }

            //save manifest to a file
            var targetDir = Path.Combine(product.CurrentUserAddInFolder, "RevitMainTool.addin");
            manifest.SaveAs(targetDir);
        }
    }
}
