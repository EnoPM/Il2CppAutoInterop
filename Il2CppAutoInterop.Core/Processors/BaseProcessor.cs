using Il2CppAutoInterop.Core.Interfaces;

namespace Il2CppAutoInterop.Core.Processors;

public abstract class BaseProcessor<TContext> : IProcessor
where TContext : class, IContext
{
    public TContext Context;

    protected BaseProcessor(TContext context)
    {
        Context = context;
    }
    
    public abstract void Process();
}