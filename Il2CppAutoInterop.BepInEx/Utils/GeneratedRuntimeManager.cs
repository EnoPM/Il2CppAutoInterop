using Il2CppAutoInterop.BepInEx.Extensions;
using Il2CppAutoInterop.BepInEx.Processors;
using Il2CppAutoInterop.Cecil.Extensions;
using Il2CppAutoInterop.Cecil.Utils;
using Il2CppAutoInterop.Core.Utils;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Il2CppAutoInterop.BepInEx.Utils;

public sealed class GeneratedRuntimeManager
{
    public readonly string BaseRuntimeNamespace;
    public readonly MethodDefinition PluginEntryPoint;
    public readonly ModuleProcessor ModuleProcessor;

    public readonly OptionalDefinition<TypeDefinition> ComponentRegistererType;
    public readonly OptionalDefinition<MethodDefinition> ComponentRegistererMethod;
    public readonly OptionalDefinition<MethodDefinition> BasicComponentRegistererMethod;
    public readonly OptionalDefinition<MethodDefinition> AdvancedComponentRegistererMethod;

    public GeneratedRuntimeManager(ModuleProcessor module)
    {
        ModuleProcessor = module;
        BaseRuntimeNamespace = Namespace(nameof(Il2CppAutoInterop), ModuleProcessor.Module.Name.Replace(".dll", string.Empty), "GeneratedRuntime");

        ComponentRegistererType = new OptionalDefinition<TypeDefinition>(CreateComponentRegistererType);
        ComponentRegistererMethod = new OptionalDefinition<MethodDefinition>(CreateComponentRegistererMethod);
        BasicComponentRegistererMethod = new OptionalDefinition<MethodDefinition>(CreateBasicComponentRegistererMethod);
        AdvancedComponentRegistererMethod =
            new OptionalDefinition<MethodDefinition>(CreateAdvancedComponentRegistererMethod);

        PluginEntryPoint = module.Module.GetBepInExPluginEntryPointMethod(module.AssemblyProcessor.Definitions) ??
                           throw new InvalidOperationException(
                               $"{module.Module.Name} doesn't contains an BepInEx Il2Cpp plugin entry point");
    }

    private TypeDefinition CreateComponentRegistererType()
    {
        var type = new TypeDefinition(
            BaseRuntimeNamespace,
            "AutoInterop",
            TypeAttributesUtility.Internal | TypeAttributesUtility.Static | TypeAttributes.Class,
            ModuleProcessor.Module.TypeSystem.Object
        );

        ModuleProcessor.Module.Types.Add(type);

        return type;
    }

    private MethodDefinition CreateComponentRegistererMethod()
    {
        var method = new MethodDefinition(
            "RegisterAllComponentsInIl2Cpp",
            MethodAttributes.Static | MethodAttributes.Assembly,
            ModuleProcessor.Module.TypeSystem.Void
        );

        ComponentRegistererType.Definition.Methods.Add(method);

        var il = method.Body.GetILProcessor();
        il.Emit(OpCodes.Ret);
        
        CallInTopOfEntryPoint(method);

        return method;
    }

    private void CallInTopOfEntryPoint(MethodReference methodToCall)
    {
        var il = PluginEntryPoint.Body.GetILProcessor();
        il.Prepend(il.Create(OpCodes.Call, methodToCall));
    }

    private MethodDefinition CreateBasicComponentRegistererMethod()
    {
        var method = new MethodDefinition(
            "RegisterComponent",
            MethodAttributes.Static | MethodAttributes.Private,
            ModuleProcessor.Module.TypeSystem.Void
        );

        var genericParameter = new GenericParameter("T", method)
        {
            Attributes = GenericParameterAttributes.NonVariant
        };
        var monoBehaviourType = ModuleProcessor.Module.ImportReference(ModuleProcessor.Definitions.MonoBehaviour);
        genericParameter.Constraints.Add(new GenericParameterConstraint(monoBehaviourType));
        method.GenericParameters.Add(genericParameter);

        var registerer = new GenericInstanceMethod(
            ModuleProcessor.Module.ImportReference(ModuleProcessor.Definitions.SimpleRegisterTypeInIl2CppMethod));
        registerer.GenericArguments.Add(genericParameter);


        var il = method.Body.GetILProcessor();
        il.Emit(OpCodes.Call, registerer);
        il.Emit(OpCodes.Ret);

        ComponentRegistererType.Definition.Methods.Add(method);

        return method;
    }

    private MethodDefinition CreateAdvancedComponentRegistererMethod()
    {
        var method = new MethodDefinition(
            "RegisterSerializedComponent",
            MethodAttributes.Static | MethodAttributes.Private,
            ModuleProcessor.Module.TypeSystem.Void
        );
        
        var genericParameter = new GenericParameter("T", method)
        {
            Attributes = GenericParameterAttributes.NonVariant
        };
        var monoBehaviourType = ModuleProcessor.Module.ImportReference(ModuleProcessor.Definitions.MonoBehaviour);
        genericParameter.Constraints.Add(new GenericParameterConstraint(monoBehaviourType));
        method.GenericParameters.Add(genericParameter);
        
        var registerer = new GenericInstanceMethod(
            ModuleProcessor.Module.ImportReference(ModuleProcessor.Definitions.RegisterTypeInIl2CppWithOptionsMethod));
        registerer.GenericArguments.Add(genericParameter);
        
        var registererOptionsConstructor = ModuleProcessor.Module.ImportReference(
            ModuleProcessor.Definitions.ClassInjectorRegisterOptionsConstructor);
        var registerOptionsSetInterfaceMethod = ModuleProcessor.Module.ImportReference(
            ModuleProcessor.Definitions.Il2CppInterfaceCollectionSetInterfaceMethod);
        var systemType = ModuleProcessor.Module.ImportReference(ModuleProcessor.Definitions.SystemType);
        var serializationInterface = ModuleProcessor.Module.ImportReference(
            ModuleProcessor.Definitions.SerializationCallbackReceiverInterface);
        var getTypeHandle = ModuleProcessor.Module.ImportReference(
            ModuleProcessor.Definitions.GetSystemTypeFromHandleMethod);
        var interfaceCollectionConstructor = ModuleProcessor.Module.ImportReference(
            ModuleProcessor.Definitions.Il2CppInterfaceCollectionConstructor);
        
        var var0 = new VariableDefinition(
            ModuleProcessor.Module.ImportReference(registererOptionsConstructor.DeclaringType));
        method.Body.Variables.Add(var0);

        var il = method.Body.GetILProcessor();
        
        il.Emit(OpCodes.Newobj, registererOptionsConstructor);
        il.Emit(OpCodes.Stloc_0);

        il.Emit(OpCodes.Ldloc_0);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Newarr, systemType);
        il.Emit(OpCodes.Dup);
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Ldtoken, serializationInterface);
        il.Emit(OpCodes.Call, getTypeHandle);
        il.Emit(OpCodes.Stelem_Ref);
        
        il.Emit(OpCodes.Newobj, interfaceCollectionConstructor);
        
        il.Emit(OpCodes.Callvirt, registerOptionsSetInterfaceMethod);
        
        il.Emit(OpCodes.Ldloc_0);
        il.Emit(OpCodes.Call, registerer);
        il.Emit(OpCodes.Ret);
        
        ComponentRegistererType.Definition.Methods.Add(method);

        return method;
    }

    private static string Namespace(params string[] fragments) => string.Join('.', fragments);
}