namespace Il2CppAutoInterop.BepInEx;

public static class BepInExIl2CppInterop
{
    public static void MakeInterop(string pluginAssemblyPath) => MakeInterop(new Config(pluginAssemblyPath));
    public static void MakeInterop(string pluginAssemblyPath, string outputAssemblyPath) => MakeInterop(new Config(pluginAssemblyPath, outputAssemblyPath));
    public static void MakeInterop(string pluginAssemblyPath, string outputAssemblyPath, string unityProjectDirectoryPath) => MakeInterop(new Config(pluginAssemblyPath, outputAssemblyPath, unityProjectDirectoryPath));

    public static void MakeInterop(Config config)
    {
        var processor = new PluginProcessor(config.InputPath)
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
        internal readonly string InputPath;
        internal readonly string? OutputPath;
        internal readonly string? UnityProjectDirectoryPath;

        public Config(string inputPath, string? outputPath = null, string? unityProjectDirectoryPath = null)
        {
            InputPath = inputPath;
            OutputPath = outputPath;
            UnityProjectDirectoryPath = unityProjectDirectoryPath;
        }
    }
}