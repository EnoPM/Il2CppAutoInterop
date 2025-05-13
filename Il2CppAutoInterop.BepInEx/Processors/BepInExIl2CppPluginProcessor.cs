using Il2CppAutoInterop.Cecil.Interfaces;
using Il2CppAutoInterop.Dependency;

namespace Il2CppAutoInterop.BepInEx.Processors;

public sealed class BepInExIl2CppPluginProcessor
{
    private readonly string _pluginAssemblyPath;
    private readonly IAssemblyLoaderContext _loader;
    private BepInExAssemblyProcessor? _processor;

    public IAssemblyDependencyManager Dependencies => _loader.Dependencies;
    
    public BepInExIl2CppPluginProcessor(string pluginAssemblyPath)
    {
        _pluginAssemblyPath = pluginAssemblyPath;
        _loader = new AssemblyLoader();
    }

    public void Preload()
    {
        var start = DateTime.UtcNow;
        Console.WriteLine($"Preloading dependencies for assembly '{Path.GetFileName(_pluginAssemblyPath)}'...");
        
        var mainAssembly = _loader.Load(_pluginAssemblyPath);
        _processor = new BepInExAssemblyProcessor(mainAssembly, _loader);
        
        var end = DateTime.UtcNow;
        var elapsed = end - start;
        
        Console.WriteLine($"Preloading ended in  {elapsed.TotalMilliseconds}ms");
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