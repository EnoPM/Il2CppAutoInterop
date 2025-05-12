using Il2CppAutoInterop.Core.Contexts;
using Il2CppAutoInterop.Core.DependencyInjection;
using Il2CppAutoInterop.Core.Interfaces;

namespace Il2CppAutoInterop.Core.BaseProcessors;

public abstract class BaseModuleProcessor : BaseProcessor, IModuleProcessor, IProcessorConsumer<ITypeProcessor, AutoInteropType>
{
    protected readonly DependencyCollection TypeProcessors;
    protected readonly AutoInteropModule Module;

    protected BaseModuleProcessor(AutoInteropModule module)
    {
        Module = module;
        TypeProcessors = new DependencyCollection(Dependencies);
    }

    public void AddProcessor<T>() where T : ITypeProcessor
    {
        TypeProcessors.Register<T>();
    }

    public IEnumerable<ITypeProcessor> GetScopedProcessors(AutoInteropType type)
    {
        var scope = TypeProcessors.CreateScope();
        scope.RegisterImmediately(type);

        return scope.GetAll<ITypeProcessor>();
    }

    public override async Task<bool> ProcessAsync()
    {
        foreach (var type in Module.Module.Types)
        {
            var typeContext = new AutoInteropType(type);

            var processors = GetScopedProcessors(typeContext);

            foreach (var processor in processors)
            {
                try
                {
                    await processor.ProcessAsync();
                }
                catch (Exception e)
                {
                    HandleFailedProcessor(processor, e);
                }
            }
        }

        return true;
    }
}