using Il2CppAutoInterop.Cecil.Interfaces;
using Il2CppAutoInterop.Core.Interfaces;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Contexts;

public class BepInExPluginAssemblyContext : BepInExPluginFileContext, IAssemblyProcessorContext
{
    public new IAssemblyLoader Loader { get; }
    public AssemblyDefinition ProcessingAssembly { get; }

    public BepInExPluginAssemblyContext(BepInExPluginFileContext context, AssemblyDefinition processingAssembly) : base(context)
    {
        Loader = context.Loader ?? throw NotLoaded();
        ProcessingAssembly = processingAssembly;
    }

    protected BepInExPluginAssemblyContext(BepInExPluginAssemblyContext context) : this(context, context.ProcessingAssembly)
    {
    }

    private static Exception NotLoaded()
    {
        return new NullReferenceException($"<{nameof(BepInExPluginFileContext)}>.Loader should not be null in {nameof(BepInExPluginAssemblyContext)} constructor.");
    }
}