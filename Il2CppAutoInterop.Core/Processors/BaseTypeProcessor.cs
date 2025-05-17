using Il2CppAutoInterop.Core.Interfaces;

namespace Il2CppAutoInterop.Core.Processors;

public abstract class BaseTypeProcessor<TContext> : BaseProcessor<TContext>
    where TContext : class, ITypeProcessorContext
{

    protected BaseTypeProcessor(TContext context) : base(context)
    {
    }
}