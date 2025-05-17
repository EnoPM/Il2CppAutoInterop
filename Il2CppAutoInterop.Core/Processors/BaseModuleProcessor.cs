using Il2CppAutoInterop.Core.Interfaces;

namespace Il2CppAutoInterop.Core.Processors;

public abstract class BaseModuleProcessor<TContext> : BaseProcessor<TContext>
    where TContext : class, IModuleProcessorContext
{

    protected BaseModuleProcessor(TContext context) : base(context)
    {
    }
}