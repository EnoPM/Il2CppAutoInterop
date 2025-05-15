using Il2CppAutoInterop.BepInEx;
using Il2CppAutoInterop.Logging;

namespace Il2CppAutoInterop.PostCompiler;

internal static class Program
{
    private static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException("Please specify the BepInEx directory path and an assembly path to load");
        }

        var bepInExDirectoryPath = args[0];
        var inputPath = args[1];
        var outputFilePath = args.Length > 2 ? args[2] : null;
        var unityProjectDirectoryPath = args.Length > 3 ? args[3] : null;

        Logger.Instance.Info($"Processing assembly: {inputPath}");

        var config = new BepInExIl2CppInterop.Config(
            bepInExDirectoryPath,
            inputPath,
            outputFilePath,
            unityProjectDirectoryPath
        );

        BepInExIl2CppInterop.MakeInterop(config);
    }
}