using Il2CppAutoInterop.BepInEx.Processors.TypeProcessors;
using Il2CppAutoInterop.BepInEx.Utils;
using Il2CppAutoInterop.Core;
using Il2CppAutoInterop.Core.Utils;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Processors;

public sealed class ModuleProcessor : IProcessor
{
    public readonly AssemblyProcessor AssemblyProcessor;
    public readonly ModuleDefinition Module;
    public readonly GeneratedRuntimeManager Runtime;
    public ResolvedDefinitions Definitions => AssemblyProcessor.Definitions;

    public ModuleProcessor(AssemblyProcessor assemblyProcessor, ModuleDefinition module)
    {
        AssemblyProcessor = assemblyProcessor;
        Module = module;
        Runtime = new GeneratedRuntimeManager(this);
    }

    public void Process()
    {
        ProcessMonoBehaviourTypes();
    }

    private void ProcessMonoBehaviourTypes()
    {
        var types = UnityUtility.GetMonoBehaviourTypes(Module, Definitions);
        foreach (var type in types)
        {
            var processor = new MonoBehaviourProcessor(this, type);
            processor.Process();
        }
    }
}