using Il2CppAutoInterop.Core.Interfaces;
using Mono.Cecil;

namespace Il2CppAutoInterop.Core.Processors;

public abstract class BaseAssemblyProcessor<TContext> : BaseProcessor<TContext>
    where TContext : class, IAssemblyProcessorContext
{

    protected BaseAssemblyProcessor(TContext context) : base(context)
    {
    }

    public override void Process()
    {
        foreach (var module in Context.ProcessingAssembly.Modules)
        {
            Process(module);
        }
    }

    protected abstract void Process(ModuleDefinition module);
}