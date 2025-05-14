using Il2CppAutoInterop.BepInEx.Utils;
using Il2CppAutoInterop.Cecil.Extensions;
using Il2CppAutoInterop.Core;
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

    public Il2CppRegistererProcessor(MonoBehaviourProcessor monoBehaviourProcessor, SerializedMonoBehaviourProcessor serializedMonoBehaviourProcessor)
    {
        MonoBehaviourProcessor = monoBehaviourProcessor;
        SerializedMonoBehaviourProcessor = serializedMonoBehaviourProcessor;
    }


    public void Process()
    {
        // WIP feature
        //TODO: Create generic method and use overloads without generic parameters
        var classLoader = Runtime.ComponentRegistererMethod.Definition;
        var ret = classLoader.Body.Instructions.Last(x => x.OpCode == OpCodes.Ret);

        var il = classLoader.Body.GetILProcessor();
        
        List<Instruction> instructions = [
            // IL_0000: newobj instance void RegisterTypeOptions::.ctor()
            il.Create(OpCodes.Newobj, Module.ImportReference(Definitions.ClassInjectorRegisterOptionsConstructor)),

            // IL_0005: stloc.0
            il.Create(OpCodes.Stloc_0),

            // IL_0006: ldloc.0
            il.Create(OpCodes.Ldloc_0),

            // IL_0007: ldc.i4.1
            il.Create(OpCodes.Ldc_I4_1),

            // IL_0008: newarr System.Type
            il.Create(OpCodes.Newarr, Module.ImportReference(Definitions.SystemType)),

            // IL_000d: dup
            il.Create(OpCodes.Dup),

            // IL_000e: ldc.i4.0
            il.Create(OpCodes.Ldc_I4_0),

            // IL_000f: ldtoken UnityEngine.ISerializationCallbackReceiver
            il.Create(OpCodes.Ldtoken, Module.ImportReference(Definitions.SerializationCallbackReceiverInterface)),

            // IL_0014: call Type.GetTypeFromHandle(RuntimeTypeHandle)
            il.Create(OpCodes.Call, Module.ImportReference(Definitions.GetSystemTypeFromHandleMethod)),

            // IL_0019: stelem.ref
            il.Create(OpCodes.Stelem_Ref),

            // IL_001a: newobj Il2CppInterfaceCollection::.ctor(IEnumerable<Type>)
            il.Create(OpCodes.Newobj, Module.ImportReference(Definitions.Il2CppInterfaceCollectionConstructor)),

            // IL_001f: ldloc.0
            il.Create(OpCodes.Ldloc_0),

            // IL_0020: callvirt RegisterTypeOptions::set_Interfaces(Il2CppInterfaceCollection)
            il.Create(OpCodes.Callvirt, Module.ImportReference(Definitions.Il2CppInterfaceCollectionSetInterfaceMethod)),

            // IL_0025: nop
            il.Create(OpCodes.Nop),
            
            il.Create(OpCodes.Ldtoken, Module.ImportReference(MonoBehaviourProcessor.ComponentType)),

            // IL_0026: ldloc.0
            il.Create(OpCodes.Ldloc_0),
            
            il.Create(OpCodes.Call, Module.ImportReference(Definitions.RegisterTypeInIl2CppWithOptionsMethod))
        ];

        // IL_002c: nop
        instructions.Add(il.Create(OpCodes.Nop));

        // Insertion des instructions avant l'instruction de retour
        foreach (var instruction in instructions)
        {
            il.InsertBefore(ret, instruction);
        }
    }
}