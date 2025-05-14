using Il2CppAutoInterop.BepInEx.Utils;
using Il2CppAutoInterop.Cecil.Interfaces;
using Il2CppAutoInterop.Core;
using Il2CppAutoInterop.Core.Utils;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Processors;

public sealed class AssemblyProcessor : IProcessor
{
    public readonly AssemblyDefinition Assembly;
    public readonly IAssemblyLoaderContext Loader;
    public readonly ResolvedDefinitions Definitions;
    
    internal AssemblyProcessor(AssemblyDefinition assembly, IAssemblyLoaderContext loader)
    {
        Assembly = assembly;
        Loader = loader;
        
        Loader.Dependencies.ProcessUnloadedDependenciesLoading();

        Definitions = new ResolvedDefinitions(loader, assembly.MainModule);
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
        var processor = new ModuleProcessor(this, module);
        processor.Process();
    }
}