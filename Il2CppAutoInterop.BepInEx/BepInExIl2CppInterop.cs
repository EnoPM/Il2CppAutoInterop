namespace Il2CppAutoInterop.BepInEx;

public static class BepInExIl2CppInterop
{
    public static void MakeInterop(Config config)
    {
        var processor = new PluginProcessor(config.BepInExDirectoryPath, config.InputPath)
        {
            UnityProjectDirectory = config.UnityProjectDirectoryPath
        };
        if (config.OutputPath != null)
        {
            processor.Run(config.OutputPath);
        }
        else
        {
            processor.Run();
        }
    }

    public class Config
    {
        internal readonly string BepInExDirectoryPath;
        internal readonly string InputPath;
        internal readonly string? OutputPath;
        internal readonly string? UnityProjectDirectoryPath;

        public Config(string bepInExDirectoryPath, string inputPath, string? outputPath = null, string? unityProjectDirectoryPath = null)
        {
            BepInExDirectoryPath = bepInExDirectoryPath;
            InputPath = inputPath;
            OutputPath = outputPath;
            UnityProjectDirectoryPath = unityProjectDirectoryPath;
        }
    }
}