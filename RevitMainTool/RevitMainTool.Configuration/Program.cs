namespace RevitMainTool.Configuration
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1) return;

            new ManifestFactory().Create(args[0]);
        }
    }
}
