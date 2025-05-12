using System.Text.Json;
using Il2CppAutoInterop.Cecil.Resolvers;
using Il2CppAutoInterop.Core.Contexts;
using Il2CppAutoInterop.Core.DependencyInjection;
using Il2CppAutoInterop.Core.Interfaces;
using Mono.Cecil;

namespace Il2CppAutoInterop.Core.BaseProcessors;

public abstract class BaseAssemblyProcessor : BaseProcessor, IAssemblyProcessor, IProcessorConsumer<IModuleProcessor, AutoInteropModule>
{
    protected readonly ResolverContext Resolver;
    protected readonly string InputPath;
    protected readonly DependencyCollection ModuleProcessors;
    protected readonly AutoInteropAssembly Assembly;

    protected BaseAssemblyProcessor(string inputPath, List<string> dependencyDirectoryPaths)
    {
        Resolver = new ResolverContext(dependencyDirectoryPaths);
        Dependencies.RegisterImmediately(Resolver);
        InputPath = inputPath;
        ModuleProcessors = new DependencyCollection(Dependencies);
        
        var assemblyDefinition = AssemblyDefinition.ReadAssembly(inputPath, Resolver.Parameters);
        Assembly = new AutoInteropAssembly(assemblyDefinition, Resolver);
        Dependencies.RegisterImmediately(Assembly);
    }

    public void AddProcessor<T>() where T : IModuleProcessor
    {
        ModuleProcessors.Register<T>();
    }

    public IEnumerable<IModuleProcessor> GetScopedProcessors(AutoInteropModule module)
    {
        var scope = ModuleProcessors.CreateScope();
        scope.RegisterImmediately(module);
        scope.Register<Naming>();
        scope.Register<MainGeneratedRuntimeType>();
        scope.ProcessRegistration();

        return scope.GetAll<IModuleProcessor>();
    }

    public override async Task<bool> ProcessAsync()
    {
        foreach (var module in Assembly.Modules)
        {
            var processors = GetScopedProcessors(module);
            
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