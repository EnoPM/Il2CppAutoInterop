using Il2CppAutoInterop.Core.Interfaces;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Contexts;

public class BepInExPluginModuleContext : BepInExPluginAssemblyContext, IModuleProcessorContext
{
    public ModuleDefinition ProcessingModule { get; }
    public InteropTypesContext InteropTypes { get; }
    public GeneratedRuntime GeneratedRuntime { get; }

    private BepInExPluginModuleContext(
        BepInExPluginAssemblyContext context,
        ModuleDefinition processingModule,
        InteropTypesContext? interopTypes,
        GeneratedRuntime? generatedRuntime) : base(context)
    {
        ProcessingModule = processingModule;
        InteropTypes = interopTypes ?? new InteropTypesContext(context.Loader, ProcessingModule);
        GeneratedRuntime = generatedRuntime ?? new GeneratedRuntime(this);
    }

    public BepInExPluginModuleContext(
        BepInExPluginAssemblyContext context,
        ModuleDefinition processingModule) : this(context, processingModule, null, null)
    {
    }

    protected BepInExPluginModuleContext(
        BepInExPluginModuleContext context) : this(context, context.ProcessingModule, context.InteropTypes, context.GeneratedRuntime)
    {
    }
}