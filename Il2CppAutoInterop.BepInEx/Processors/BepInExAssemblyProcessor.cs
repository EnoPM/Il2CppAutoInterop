using Il2CppAutoInterop.BepInEx.Utils;
using Il2CppAutoInterop.Cecil.Interfaces;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Processors;

public sealed class BepInExAssemblyProcessor
{
    public readonly AssemblyDefinition Assembly;
    public readonly IAssemblyLoaderContext Loader;
    public readonly DefinitionContext Definitions;
    
    internal BepInExAssemblyProcessor(AssemblyDefinition assembly, IAssemblyLoaderContext loader)
    {
        Assembly = assembly;
        Loader = loader;
        
        Loader.Dependencies.ProcessUnloadedDependenciesLoading();

        Definitions = new DefinitionContext(loader, assembly.MainModule);
    }

    public void Process()
    {
        foreach (var module in Assembly.Modules)
        {
            Process(module);
        }
    }

    private void Process(ModuleDefinition module)
    {
        var processor = new BepInExModuleProcessor(this, module);
        processor.Process();
    }
}