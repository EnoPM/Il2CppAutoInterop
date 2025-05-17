using Il2CppAutoInterop.Core.Interfaces;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Contexts;

public class BepInExPluginMonoBehaviourContext : BepInExPluginModuleContext, ITypeProcessorContext
{
    public TypeDefinition ProcessingType { get; }

    public BepInExPluginMonoBehaviourContext(BepInExPluginModuleContext context, TypeDefinition processingType) : base(context)
    {
        ProcessingType = processingType;
    }

    protected BepInExPluginMonoBehaviourContext(BepInExPluginMonoBehaviourContext context) : this(context, context.ProcessingType)
    {
    }
}