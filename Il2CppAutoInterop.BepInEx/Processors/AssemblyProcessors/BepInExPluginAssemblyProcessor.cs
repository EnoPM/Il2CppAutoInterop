using Il2CppAutoInterop.BepInEx.Contexts;
using Il2CppAutoInterop.BepInEx.Processors.ModuleProcessors;
using Il2CppAutoInterop.Core.Processors;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Processors.AssemblyProcessors;

public class BepInExPluginAssemblyProcessor : BaseAssemblyProcessor<BepInExPluginAssemblyContext>
{
    public BepInExPluginAssemblyProcessor(BepInExPluginAssemblyContext context) : base(context)
    {
    }
    
    protected override void Process(ModuleDefinition module)
    {
        var moduleContext = new BepInExPluginModuleContext(Context, module);

        var processor = new BepInExPluginModuleProcessor(moduleContext);
        processor.Process();
    }
}