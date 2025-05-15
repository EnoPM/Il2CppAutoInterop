using Il2CppAutoInterop.BepInEx;
using Il2CppAutoInterop.BepInEx.Extensions;
using Il2CppAutoInterop.BepInEx.Processors;
using Il2CppAutoInterop.Logging;

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
        var outputFilePath = args.Length > 1 ? args[1] : inputPath;

        Logger.Instance.Info($"Processing assembly: {inputPath}");
        
        BepInExIl2CppInterop.MakeInterop(inputPath, outputFilePath);
    }
}