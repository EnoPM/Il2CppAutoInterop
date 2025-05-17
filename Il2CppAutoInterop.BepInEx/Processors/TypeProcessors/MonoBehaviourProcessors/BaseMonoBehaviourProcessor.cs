using Il2CppAutoInterop.BepInEx.Contexts;
using Il2CppAutoInterop.Core.Processors;

namespace Il2CppAutoInterop.BepInEx.Processors.TypeProcessors.MonoBehaviourProcessors;

public abstract class BaseMonoBehaviourProcessor<TContext> : BaseTypeProcessor<TContext>
    where TContext : BepInExPluginMonoBehaviourContext
{
    protected BaseMonoBehaviourProcessor(TContext context) : base(context)
    {
    }
}

public abstract class BaseMonoBehaviourProcessor : BaseMonoBehaviourProcessor<BepInExPluginMonoBehaviourContext>
{
    protected BaseMonoBehaviourProcessor(BepInExPluginMonoBehaviourContext context) : base(context)
    {
    }
}

