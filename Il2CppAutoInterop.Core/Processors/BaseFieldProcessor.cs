using Il2CppAutoInterop.Core.Interfaces;

namespace Il2CppAutoInterop.Core.Processors;

public abstract class BaseFieldProcessor<TContext> : BaseProcessor<TContext>
    where TContext : class, IFieldProcessorContext
{

    protected BaseFieldProcessor(TContext context) : base(context)
    {
    }
}