using Il2CppAutoInterop.BepInEx.Utils;
using Il2CppAutoInterop.Core;
using Il2CppAutoInterop.Core.Utils;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Il2CppAutoInterop.BepInEx.Processors.TypeProcessors;

public sealed class Il2CppRegistererProcessor : IProcessor
{
    public readonly MonoBehaviourProcessor MonoBehaviourProcessor;
    public readonly SerializedMonoBehaviourProcessor SerializedMonoBehaviourProcessor;

    public GeneratedRuntimeManager Runtime => MonoBehaviourProcessor.ModuleProcessor.Runtime;
    public ModuleDefinition Module => MonoBehaviourProcessor.ModuleProcessor.Module;
    public ResolvedDefinitions Definitions => MonoBehaviourProcessor.Definitions;

    public Il2CppRegistererProcessor(MonoBehaviourProcessor monoBehaviourProcessor,
        SerializedMonoBehaviourProcessor serializedMonoBehaviourProcessor)
    {
        MonoBehaviourProcessor = monoBehaviourProcessor;
        SerializedMonoBehaviourProcessor = serializedMonoBehaviourProcessor;
    }


    private OptionalDefinition<MethodDefinition> GetBaseRegisterer()
    {
        if (!MonoBehaviourProcessor.UseUnitySerializationInterface || SerializedMonoBehaviourProcessor.SerializedFields.Count == 0)
        {
            return Runtime.BasicComponentRegistererMethod;
        }

        return Runtime.AdvancedComponentRegistererMethod;
    }

    public void Process()
    {
        var classLoader = Runtime.ComponentRegistererMethod.Definition;
        var il = classLoader.Body.GetILProcessor();
        
        var ret = il.Body.Instructions.First(x => x.OpCode == OpCodes.Ret);

        var baseRegisterer = GetBaseRegisterer();
        var registerer = new GenericInstanceMethod(Module.ImportReference(baseRegisterer.Definition));
        registerer.GenericArguments.Add(MonoBehaviourProcessor.ComponentType);
        
        il.InsertBefore(ret, il.Create(OpCodes.Call, registerer));
    }
}