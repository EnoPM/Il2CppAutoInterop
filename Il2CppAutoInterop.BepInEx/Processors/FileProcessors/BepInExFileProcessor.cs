using Il2CppAutoInterop.BepInEx.Contexts;
using Il2CppAutoInterop.BepInEx.Processors.AssemblyProcessors;
using Il2CppAutoInterop.BepInEx.Utils;
using Il2CppAutoInterop.Cecil.Interfaces;
using Il2CppAutoInterop.Common;
using Il2CppAutoInterop.Core.Processors;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Processors.FileProcessors;

public sealed class BepInExFileProcessor : BaseFileProcessor<BepInExPluginFileContext>
{
    public BepInExPluginAssemblyProcessor? AssemblyProcessor { get; private set; }
    
    public BepInExFileProcessor(BepInExPluginFileContext context) : base(context)
    {
        
    }

    protected override IAssemblyLoader CreateLoader()
    {
        var loader = base.CreateLoader();

        var bepInExParentDirectory = Path.GetDirectoryName(Context.BepInExDirectoryPath);
        if (bepInExParentDirectory == null || !Directory.Exists(bepInExParentDirectory))
        {
            throw new Exception($"Unable to locate BepInEx parent directory from {Context.BepInExDirectoryPath}");
        }
        var dotnetDirectory = Path.Combine(bepInExParentDirectory, "dotnet");
        if (!Directory.Exists(dotnetDirectory))
        {
            throw new Exception($"Unable to locate 'dotnet' directory at {dotnetDirectory}");
        }
        
        loader.Dependencies.AddDirectory(dotnetDirectory);

        foreach (var directoryName in BepInExUtility.Directories)
        {
            var dependencyDirectoryPath = Path.Combine(Context.BepInExDirectoryPath, directoryName);
            loader.Dependencies.AddDirectory(dependencyDirectoryPath);
        }

        return loader;
    }

    public override void Load()
    {
        base.Load();
        var context = new BepInExPluginAssemblyContext(Context, LoadedAssembly!);
        AssemblyProcessor = new BepInExPluginAssemblyProcessor(context);
    }

    protected override void Process(AssemblyDefinition assembly)
    {
        if (AssemblyProcessor == null)
        {
            throw ExceptionFactory.AssemblyNotYetLoaded<BepInExFileProcessor>(nameof(Process), Context.InputPath);
        }
        AssemblyProcessor.Process();
    }
    
    public void RandomizeAssemblyVersion()
    {
        if (LoadedAssembly == null)
        {
            throw new Exception($"Assembly {Context.InputPath} is not yet loaded");
        }

        var random = new Random();
        var currentVersion = LoadedAssembly.Name.Version;
        var version = new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build, random.Next(10000, 99999));
        LoadedAssembly.Name.Version = version;
    }
}