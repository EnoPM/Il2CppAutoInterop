using Il2CppAutoInterop.BepInEx.Processors.MonoBehaviourComponents;
using Il2CppAutoInterop.BepInEx.Utils;
using Il2CppAutoInterop.Core.Utils;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Processors;

public sealed class ModuleProcessor
{
    public readonly AssemblyProcessor AssemblyProcessor;
    public readonly ModuleDefinition Module;
    public readonly GeneratedRuntimeType Runtime;
    public DefinitionContext Definitions => AssemblyProcessor.Definitions;

    public ModuleProcessor(AssemblyProcessor assemblyProcessor, ModuleDefinition module)
    {
        AssemblyProcessor = assemblyProcessor;
        Module = module;
        Runtime = new GeneratedRuntimeType(this);
    }

    public void Process()
    {
        using (new TimedExecution($"Processing MonoBehaviour components for module {Module.Name}", ConsoleColor.DarkYellow))
        {
            ProcessMonoBehaviourTypes();
        }
    }

    private void ProcessMonoBehaviourTypes()
    {
        var types = UnityUtility.GetMonoBehaviourTypes(Module, Definitions);
        foreach (var type in types)
        {
            var processor = new MonoBehaviourComponentProcessor(this, type);
            processor.Process();
        }
    }
}