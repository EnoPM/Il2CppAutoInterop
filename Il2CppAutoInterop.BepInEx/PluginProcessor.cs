using Il2CppAutoInterop.BepInEx.Extensions;
using Il2CppAutoInterop.BepInEx.Processors;
using Il2CppAutoInterop.Cecil.Interfaces;
using Il2CppAutoInterop.Core;
using Il2CppAutoInterop.Core.Utils;
using Il2CppAutoInterop.Logging;

namespace Il2CppAutoInterop.BepInEx;

public sealed class PluginProcessor : IProcessor
{
    private readonly string _bepInExDirectoryPath;
    private readonly string _pluginAssemblyPath;
    private readonly IAssemblyLoaderContext _loader;
    private AssemblyProcessor? _processor;

    public IAssemblyDependencyManager Dependencies => _loader.Dependencies;
    
    public string? UnityProjectDirectory { get; set; }
    
    public PluginProcessor(string bepInExDirectoryPath, string pluginAssemblyPath)
    {
        _bepInExDirectoryPath = bepInExDirectoryPath;
        _pluginAssemblyPath = pluginAssemblyPath;
        _loader = new AssemblyLoader();
    }

    public void Run() => Run(_pluginAssemblyPath);

    public void Run(string outputFilePath)
    {
        using (new TimedExecution("Register BepInEx plugin dependencies"))
        {
            Dependencies.RegisterBepInExPlugin(_bepInExDirectoryPath, _pluginAssemblyPath);
        }

        using (new TimedExecution($"Preloading dependencies for assembly {Path.GetFileName(_pluginAssemblyPath)}", ConsoleColor.Yellow))
        {
            Preload();
        }

        using (new TimedExecution($"Processing {_pluginAssemblyPath}", ConsoleColor.Magenta))
        {
            Process();
        }
        
        #if DEBUG
        RandomizeAssemblyVersion();
        #endif

        using (new TimedExecution($"Saving changes to {outputFilePath}", ConsoleColor.Green))
        {
            Save(outputFilePath);
        }
    }

    public void Preload()
    {
        var mainAssembly = _loader.Load(_pluginAssemblyPath);
        _processor = new AssemblyProcessor(this, mainAssembly, _loader);
    }

    public void Process()
    {
        if (_processor == null)
        {
            throw new InvalidOperationException($"Assembly {_pluginAssemblyPath} is not yet loaded");
        }
        _processor.Process();
    }
    
    public void Save(string destinationFilePath)
    {
        if (_processor == null)
        {
            throw new InvalidOperationException($"Assembly {_pluginAssemblyPath} is not yet loaded");
        }
        _processor.Assembly.Write(destinationFilePath);
    }

    public void RandomizeAssemblyVersion()
    {
        if (_processor == null)
        {
            throw new InvalidOperationException($"Assembly {_pluginAssemblyPath} is not yet loaded");
        }

        var random = new Random();
        var v = _processor.Assembly.Name.Version;
        var version = new Version(v.Major, v.Minor, v.Build, random.Next(10000, 99999));
        _processor.Assembly.Name.Version = version;
    }
}