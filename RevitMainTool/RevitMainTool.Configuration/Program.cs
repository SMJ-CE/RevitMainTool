using Squirrel;
using System;
using System.Threading.Tasks;

namespace RevitMainTool.Configuration
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("hello Eivind");
            Console.ReadLine();
            //using (var mgr = new UpdateManager("C:\\installer_test"))
            //{
            //    await mgr.UpdateApp();
            //}
            //if (args.Length != 1) return;

            //new ManifestFactory().Create(args[0]);
        }
    }
}
