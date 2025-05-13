using Il2CppAutoInterop.BepInEx.Extensions;
using Il2CppAutoInterop.BepInEx.Processors;
using Il2CppAutoInterop.Cecil.Interfaces;
using Il2CppAutoInterop.Core;
using Il2CppAutoInterop.Core.Utils;

namespace Il2CppAutoInterop.BepInEx;

public sealed class PluginProcessor
{
    private readonly string _pluginAssemblyPath;
    private readonly IAssemblyLoaderContext _loader;
    private AssemblyProcessor? _processor;

    public IAssemblyDependencyManager Dependencies => _loader.Dependencies;
    
    public PluginProcessor(string pluginAssemblyPath)
    {
        _pluginAssemblyPath = pluginAssemblyPath;
        _loader = new AssemblyLoader();
    }

    public void Run() => Run(_pluginAssemblyPath);

    public void Run(string outputFilePath)
    {
        using (new TimedExecution("Register BepInEx plugin dependencies"))
        {
            Dependencies.RegisterBepInExPlugin(_pluginAssemblyPath);
        }

        using (new TimedExecution($"Preloading dependencies for assembly {Path.GetFileName(_pluginAssemblyPath)}", ConsoleColor.Yellow))
        {
            Preload();
        }

        using (new TimedExecution($"Process {nameof(PluginProcessor)}", ConsoleColor.Magenta))
        {
            Process();
        }
        
        IncrementAssemblyVersion();

        using (new TimedExecution("Saving changes to input assembly", ConsoleColor.Green))
        {
            Save(outputFilePath);
        }
    }

    public void Preload()
    {
        var mainAssembly = _loader.Load(_pluginAssemblyPath);
        _processor = new AssemblyProcessor(mainAssembly, _loader);
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
    
    public void IncrementAssemblyVersion()
    {
        if (_processor == null)
        {
            throw new InvalidOperationException($"Assembly {_pluginAssemblyPath} is not yet loaded");
        }
        var v = _processor.Assembly.Name.Version;
        var version = new Version(v.Major, v.Minor, v.Build, v.Revision + 1);
        _processor.Assembly.Name.Version = version;
    }
}