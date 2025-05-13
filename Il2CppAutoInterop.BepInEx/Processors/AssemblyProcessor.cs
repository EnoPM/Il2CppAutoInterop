using Il2CppAutoInterop.BepInEx.Utils;
using Il2CppAutoInterop.Cecil.Interfaces;
using Il2CppAutoInterop.Core.Utils;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Processors;

public sealed class AssemblyProcessor
{
    public readonly AssemblyDefinition Assembly;
    public readonly IAssemblyLoaderContext Loader;
    public readonly DefinitionContext Definitions;
    
    internal AssemblyProcessor(AssemblyDefinition assembly, IAssemblyLoaderContext loader)
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
            using (new TimedExecution($"Processing module {module.Name}", ConsoleColor.DarkCyan))
            {
                Process(module);
            }
        }
    }

    private void Process(ModuleDefinition module)
    {
        var processor = new ModuleProcessor(this, module);
        processor.Process();
    }
}