using Il2CppAutoInterop.Cecil.Resolvers;
using Il2CppAutoInterop.Core.DependencyInjection;
using Il2CppAutoInterop.Core.Interfaces;

namespace Il2CppAutoInterop.Core.BaseProcessors;

public abstract class BaseProcessor : IBaseProcessor
{
    
    protected readonly DependencyCollection Dependencies = new();
    
    protected void Register<T>(T instance) where T : class => Dependencies.RegisterImmediately(instance);
    protected void Register<T>() where T : class => Dependencies.Register<T>();
    protected void CompileDependencies() => Dependencies.ProcessRegistration();
    
    public abstract Task<bool> ProcessAsync();
    
    protected static void HandleFailedProcessor(IBaseProcessor processor, Exception exception)
    {
        Console.WriteLine($"{processor.GetType().Name} failed: {exception.Message}");
    }
}