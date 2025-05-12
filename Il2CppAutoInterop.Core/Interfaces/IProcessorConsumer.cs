namespace Il2CppAutoInterop.Core.Interfaces;

public interface IProcessorConsumer<TProcessor, TAutoInteropContext>
    where TProcessor : IBaseProcessor
    where TAutoInteropContext : IAutoInteropContext
{
    public void AddProcessor<T>() where T : TProcessor;
    public IEnumerable<TProcessor> GetScopedProcessors(TAutoInteropContext context);
}