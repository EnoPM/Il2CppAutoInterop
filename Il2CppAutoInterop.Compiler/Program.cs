using Il2CppAutoInterop.BepInEx;
using Il2CppAutoInterop.BepInEx.Extensions;
using Il2CppAutoInterop.BepInEx.Processors;

namespace Il2CppAutoInterop.Compiler;

internal static class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("Please specify an assembly path to load");
        }

        var inputPath = args[0];
        var outputFilePath = Path.Combine(Path.GetDirectoryName(inputPath)!, "output.dll");

        Console.WriteLine($"Processing assembly: {inputPath}");

        var processor = new PluginProcessor(inputPath);
        processor.Run(outputFilePath);
    }
}