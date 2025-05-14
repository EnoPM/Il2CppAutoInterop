using Il2CppAutoInterop.Cecil.Extensions;
using Il2CppAutoInterop.Core;
using Il2CppAutoInterop.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Il2CppAutoInterop.BepInEx.Processors.TypeProcessors;

public sealed class IntPtrConstructorProcessor : IProcessor
{
    public readonly MonoBehaviourProcessor MonoBehaviourProcessor;
    public ModuleDefinition Module => MonoBehaviourProcessor.ModuleProcessor.Module;
    public TypeDefinition ComponentType => MonoBehaviourProcessor.ComponentType;

    public IntPtrConstructorProcessor(MonoBehaviourProcessor behaviourProcessor)
    {
        MonoBehaviourProcessor = behaviourProcessor;
    }

    public void Process()
    {
        if (HasIntPtrConstructor()) return;
        var defaultConstructor = FindDefaultConstructor();
        if (defaultConstructor == null)
        {
            Logger.Instance.Info($"[{ComponentType.Name}] No default constructor found, creating a new one...");
            CreateIntPtrConstructor();
        }
        else
        {
            Logger.Instance.Info($"[{ComponentType.Name}] Default constructor found, updating it...");
            UpdateExistingConstructor(defaultConstructor);
        }
    }

    private void CreateIntPtrConstructor()
    {
        var attributes = MethodAttributes.HideBySig | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName;
        if (ComponentType.IsAbstract)
        {
            attributes |= MethodAttributes.Family;
        }
        else
        {
            attributes |= MethodAttributes.Public;
        }

        var constructor = new MethodDefinition(
            ".ctor",
            attributes,
            Module.TypeSystem.Void
        );

        var parameter = new ParameterDefinition("ptr", ParameterAttributes.None, Module.TypeSystem.IntPtr);
        constructor.Parameters.Add(parameter);

        ComponentType.Methods.Add(constructor);

        
        var il = constructor.Body.GetILProcessor();

        if (!ComponentType.TryFindNearestMethod(AncestorConstructorFinder, out var parentConstructorDefinition))
        {
            throw new Exception($"Unable to find IntPtr ancestor constructor for {ComponentType.Name}");
        }

        Logger.Instance.Info($"{ComponentType.Name} : {parentConstructorDefinition.FullName}");

        var parentConstructor = Module.ImportReference(parentConstructorDefinition.Resolve());

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Call, parentConstructor);

        il.Emit(OpCodes.Ret);
    }

    private void UpdateExistingConstructor(MethodDefinition constructor)
    {
        var firstCallInstruction = constructor.Body.Instructions.FirstOrDefault(x => x.OpCode == OpCodes.Call);
        if (firstCallInstruction == null)
        {
            throw new Exception($"Unable to find call instruction for {constructor.FullName}");
        }
        if (firstCallInstruction.Operand is not MethodReference methodReference)
        {
            throw new Exception($"First call instruction for {constructor.FullName} isn't a MethodReference");
        }
        var ancestorConstructor = methodReference.Resolve();
        if (!ancestorConstructor.IsConstructor)
        {
            throw new Exception($"First call instruction for {constructor.FullName} isn't a Constructor");
        }
        MethodDefinition? validParentConstructor = null;
        if (ancestorConstructor.HasParameters)
        {
            if (ancestorConstructor.Parameters.Count != 1 && ancestorConstructor.Parameters.First().ParameterType.FullName != Module.TypeSystem.IntPtr.FullName)
            {
                throw new Exception($"Invalid ancestor constructor '{ancestorConstructor.FullName}' for '{constructor.FullName}': invalid parameters");
            }
            validParentConstructor = ancestorConstructor;
        }
        if (validParentConstructor == null)
        {
            if (!ComponentType.TryFindNearestMethod(AncestorConstructorFinder, out validParentConstructor))
            {
                throw new Exception($"Unable to find a valid ancestor constructor for {constructor.FullName}");
            }
        }
        
        constructor.Parameters.Add(new ParameterDefinition("ptr", ParameterAttributes.None, Module.TypeSystem.IntPtr));
        var il = constructor.Body.GetILProcessor();
        il.InsertBefore(firstCallInstruction, Instruction.Create(OpCodes.Ldarg_1));
        firstCallInstruction.Operand = Module.ImportReference(validParentConstructor);
    }

    private MethodDefinition? FindDefaultConstructor() => ComponentType.Methods.FirstOrDefault(x => x.IsConstructor && !x.HasParameters);

    private bool AncestorConstructorFinder(MethodDefinition method)
    {
        if (method.DeclaringType.FullName == ComponentType.FullName)
        {
            return false;
        }

        return IsIntPtrConstructor(method);
    }

    private bool HasIntPtrConstructor()
    {
        return ComponentType.Methods.Any(IsIntPtrConstructor);
    }

    private bool IsIntPtrConstructor(MethodDefinition method) => method.IsConstructor
                                                                 && method.HasParameters
                                                                 && method.Parameters.Count == 1
                                                                 && method.Parameters[0].ParameterType.FullName == Module.TypeSystem.IntPtr.FullName;
}