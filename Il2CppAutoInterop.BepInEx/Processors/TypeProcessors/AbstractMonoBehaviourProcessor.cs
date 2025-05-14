using Il2CppAutoInterop.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Il2CppAutoInterop.BepInEx.Processors.TypeProcessors;

public sealed class AbstractMonoBehaviourProcessor : IProcessor
{
    public readonly MonoBehaviourProcessor MonoBehaviourProcessor;
    public ModuleDefinition Module => MonoBehaviourProcessor.ModuleProcessor.Module;
    public ResolvedDefinitions Definitions => MonoBehaviourProcessor.ModuleProcessor.Definitions;
    public TypeDefinition ComponentType => MonoBehaviourProcessor.ComponentType;
    
    public AbstractMonoBehaviourProcessor(MonoBehaviourProcessor behaviourProcessor)
    {
        MonoBehaviourProcessor = behaviourProcessor;
    }

    public void Process()
    {
        foreach (var method in ComponentType.Methods)
        {
            if (!method.IsAbstract) continue;
            ConvertAbstractToVirtual(method);
        }
    }

    private void ConvertAbstractToVirtual(MethodDefinition method)
    {
        method.IsAbstract = false;
        method.IsVirtual = true;
        
        var il = method.Body.GetILProcessor();
        il.Emit(OpCodes.Newobj, Module.ImportReference(Definitions.NotImplementedExceptionConstructor));
        il.Emit(OpCodes.Throw);
    }
}