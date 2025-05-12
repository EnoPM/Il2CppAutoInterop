namespace Il2CppAutoInterop.BepInEx;

public sealed class BepInExContext
{
    public readonly string InputPath;
    public readonly string OutputPath;
    public readonly List<string> DependencyDirectories;

    public BepInExContext(string inputPath)
    {
        InputPath = inputPath;
        
        var managedPluginDirectory = Path.GetDirectoryName(InputPath)!;
        var bepInExDirectory = Path.GetDirectoryName(managedPluginDirectory)!;
        
        //OutputPath = outputPath;
        OutputPath = inputPath;
        DependencyDirectories = [
            managedPluginDirectory,
            Path.Combine(bepInExDirectory, "core"),
            Path.Combine(bepInExDirectory, "plugins"),
            Path.Combine(bepInExDirectory, "interop"),
            Path.Combine(bepInExDirectory, "patchers"),
            Path.Combine(bepInExDirectory, "dotnet")
        ];
    }
}