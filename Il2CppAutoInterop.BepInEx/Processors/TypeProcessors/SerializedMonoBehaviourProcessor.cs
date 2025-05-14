using Il2CppAutoInterop.BepInEx.Processors.FieldProcessors;
using Il2CppAutoInterop.BepInEx.Utils;
using Il2CppAutoInterop.Cecil.Extensions;
using Il2CppAutoInterop.Core;
using Il2CppAutoInterop.Core.Utils;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Il2CppAutoInterop.BepInEx.Processors.TypeProcessors;

public sealed class SerializedMonoBehaviourProcessor : IProcessor
{
    public readonly MonoBehaviourProcessor MonoBehaviourProcessor;
    public readonly List<SerializedFieldProcessor> SerializedFields;
    
    public ResolvedDefinitions Definitions => MonoBehaviourProcessor.Definitions;
    public GeneratedRuntimeManager Runtime => MonoBehaviourProcessor.ModuleProcessor.Runtime;
    public ModuleDefinition Module => MonoBehaviourProcessor.ModuleProcessor.Module;
    public TypeDefinition ComponentType => MonoBehaviourProcessor.ComponentType;
    
    public readonly OptionalDefinition<MethodDefinition> DeserializationMethod;
    
    public SerializedMonoBehaviourProcessor(MonoBehaviourProcessor monoBehaviourProcessor)
    {
        MonoBehaviourProcessor = monoBehaviourProcessor;
        SerializedFields = UnityUtility.GetSerializedFields(MonoBehaviourProcessor.ComponentType, MonoBehaviourProcessor.Definitions)
            .Select(x => new SerializedFieldProcessor(this, x))
            .ToList();
        DeserializationMethod = new OptionalDefinition<MethodDefinition>(CreateDeserializationMethod);
    }
    

    public void Process()
    {
        if (SerializedFields.Count == 0)
        {
            return;
        }
        foreach (var processor in SerializedFields)
        {
            processor.Process();
        }
        AddDeserializeMethodCall();

        // TODO: Test if deserialization method can be called with ISerializationCallbackReceiver.OnAfterDeserialize method
    }

    private void AddDeserializeMethodCall()
    {
        var awakeMethod = FindOrCreateAwakeMethod(out var parentAwakeMethod);
        var il = awakeMethod.Body.GetILProcessor();
        il.Prepend([
            il.Create(OpCodes.Ldarg_0),
            il.Create(OpCodes.Call, Module.ImportReference(DeserializationMethod.Definition))
        ]);
        if (parentAwakeMethod == null) return;
        var hasParentAwakeMethodCall = awakeMethod.Body.Instructions
            .Any(x => x.OpCode == OpCodes.Call && x.Operand is MethodReference xMethod && xMethod.FullName == parentAwakeMethod.FullName);
        if (hasParentAwakeMethodCall) return;
        il.Prepend([
            il.Create(OpCodes.Ldarg_0),
            il.Create(OpCodes.Call, Module.ImportReference(parentAwakeMethod))
        ]);
    }

    private MethodDefinition FindOrCreateAwakeMethod(out MethodDefinition? parentAwakeMethodResult)
    {
        if (!ComponentType.TryFindNearestMethod(NearestAwakeMethodFinder, out var nearestAwakeMethod))
        {
            // No awake method found in parent or in current class
            var newAwakeMethod = CreateEmptyAwakeMethod(MethodAttributes.Private);
            ComponentType.Methods.Add(newAwakeMethod);
            parentAwakeMethodResult = null;
            return newAwakeMethod;
        }

        if (nearestAwakeMethod.DeclaringType.FullName == ComponentType.FullName)
        {
            // Awake method found in current class
            var parentAwakeMethod = EnsureParentAwakeMethodAccess();
            if (parentAwakeMethod != null)
            {
                // Parent class has a generated Awake method. So we need to make current Awake an override and ensure parent Awake method is call in child Awake method override.
                nearestAwakeMethod.IsPrivate = false;
                nearestAwakeMethod.IsPublic = parentAwakeMethod.IsPublic;
                nearestAwakeMethod.IsFamily = parentAwakeMethod.IsFamily;
                nearestAwakeMethod.IsVirtual = true;
                nearestAwakeMethod.IsHideBySig = true;
            }
            parentAwakeMethodResult = parentAwakeMethod;
            return nearestAwakeMethod;
        }
        
        // Awake method found in parent class so we need to fix the access of it
        if (nearestAwakeMethod.IsPrivate)
        {
            nearestAwakeMethod.IsPrivate = false;
            nearestAwakeMethod.IsFamily = true;
        }
        if (!nearestAwakeMethod.IsVirtual)
        {
            nearestAwakeMethod.IsVirtual = true;
        }
        var attributes = MethodAttributes.Virtual | MethodAttributes.HideBySig;
        if (nearestAwakeMethod.IsFamily)
        {
            attributes |= MethodAttributes.Family;
        }
        
        var awakeMethod = CreateEmptyAwakeMethod(attributes);
        ComponentType.Methods.Add(awakeMethod);
        
        parentAwakeMethodResult = nearestAwakeMethod;
        
        return awakeMethod;
    }
    
    private MethodDefinition? EnsureParentAwakeMethodAccess()
    {
        if (!ComponentType.TryFindNearestMethod(NearestParentAwakeMethodFinder, out var parentAwakeMethod))
        {
            return null;
        }
        if (parentAwakeMethod.IsPrivate)
        {
            parentAwakeMethod.IsPrivate = false;
            parentAwakeMethod.IsFamily = true;
        }
        if (!parentAwakeMethod.IsVirtual)
        {
            parentAwakeMethod.IsVirtual = true;
        }

        return parentAwakeMethod;
    }

    private static bool NearestAwakeMethodFinder(MethodDefinition method)
    {
        return method.Name == "Awake" && !method.HasParameters;
    }
    
    private bool NearestParentAwakeMethodFinder(MethodDefinition method)
    {
        return method.DeclaringType.FullName != ComponentType.FullName && NearestAwakeMethodFinder(method);
    }
    
    private MethodDefinition CreateEmptyAwakeMethod(MethodAttributes attributes)
    {
        var awakeMethod = new MethodDefinition("Awake", attributes, Module.TypeSystem.Void);

        var il = awakeMethod.Body.GetILProcessor();
        il.Emit(OpCodes.Ret);

        return awakeMethod;
    }
    
    private MethodDefinition CreateDeserializationMethod()
    {
        var method = new MethodDefinition(
            $"__{nameof(Il2CppAutoInterop)}_${ComponentType.Name}_AfterDeserializeMethod",
            MethodAttributes.Private,
            Module.TypeSystem.Void
        );
        
        ComponentType.Methods.Add(method);

        var il = method.Body.GetILProcessor();

        foreach (var processor in SerializedFields)
        {
            processor.AddDeserializationIlCode(il);
        }

        il.Emit(OpCodes.Ret);

        return method;
    }
}