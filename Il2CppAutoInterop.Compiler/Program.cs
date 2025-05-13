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

        Console.WriteLine($"Processing assembly: {inputPath}");

        var interopProcessor = new BepInExIl2CppPluginProcessor(inputPath);
        
        interopProcessor.Dependencies.RegisterBepInExPlugin(inputPath);
        
        interopProcessor.Preload();
        interopProcessor.Process();
        
        interopProcessor.IncrementAssemblyVersion();
        interopProcessor.Save(inputPath);
    }
}