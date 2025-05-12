using Il2CppAutoInterop.Core.BaseProcessors;

namespace Il2CppAutoInterop.BepInEx;

public class PluginAssemblyProcessor : BaseAssemblyProcessor
{
    private readonly BepInExContext _context;

    public PluginAssemblyProcessor(BepInExContext context) : base(context.InputPath, context.DependencyDirectories)
    {
        Dependencies.RegisterImmediately(_context = context);
        
        AddProcessor<PluginModuleProcessor>();
    }

    public override async Task<bool> ProcessAsync()
    {
        var processingResult = await base.ProcessAsync();
        if (!processingResult) return false;
        
        Save();
        
        return true;
    }

    private void Save()
    {
        var v = Assembly.Assembly.Name.Version;
        var version = new Version(v.Major, v.Minor, v.Build, v.Revision + 1);
        Assembly.Assembly.Name.Version = version;
        
        Assembly.Assembly.Write(_context.OutputPath);
        Console.WriteLine($"Saved assembly to {_context.OutputPath}");
    }
}