using Il2CppAutoInterop.Common;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Contexts;

public class BepInExPluginSerializationContext : BepInExPluginMonoBehaviourContext
{
    public Loadable<MethodDefinition> DeserializationMethod { get; }

    public BepInExPluginSerializationContext(BepInExPluginMonoBehaviourContext context, Loadable<MethodDefinition> deserializationMethod) : base(context)
    {
        DeserializationMethod = deserializationMethod;
    }

    protected BepInExPluginSerializationContext(BepInExPluginSerializationContext context) : this(context, context.DeserializationMethod)
    {
    }
}