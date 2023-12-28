using Squirrel;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RevitMainTool.Configuration
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var cwd = Directory.GetCurrentDirectory();
            var content = "test" + string.Join(", ", args);
            
            try {
                File.WriteAllText("C:\\Users\\eev_9\\source\\repos\\SMJTools\\RevitMainTool\\RevitMainTool\\RevitMainTool.Configuration\\Releases\\log.txt", content);

                new ManifestFactory().Create(cwd);
            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine(content);
            Console.ReadLine();
        }
    }
}
