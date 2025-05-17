using Il2CppAutoInterop.BepInEx.Extensions;
using Il2CppAutoInterop.Cecil.Extensions;
using Il2CppAutoInterop.Cecil.Utils;
using Il2CppAutoInterop.Common;
using Il2CppAutoInterop.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Il2CppAutoInterop.BepInEx.Contexts;

public sealed class GeneratedRuntime : BaseRuntimeManager
{
    private readonly BepInExPluginModuleContext _context;
    private readonly Loadable<MethodDefinition> _pluginEntryPoint;
    private readonly Loadable<TypeDefinition> _componentRegistererType;
    public Loadable<MethodDefinition> ComponentRegistererMethod { get; }
    
    public Loadable<MethodDefinition> SimpleComponentRegisterer { get; }
    public Loadable<MethodDefinition> InterfaceComponentRegisterer { get; }
    
    public GeneratedRuntime(BepInExPluginModuleContext context) : base(context.ProcessingModule.Name)
    {
        _context = context;

        _pluginEntryPoint = new Loadable<MethodDefinition>(FindEntryPoint);
        
        _componentRegistererType = new Loadable<TypeDefinition>(CreateComponentRegistererType);
        SimpleComponentRegisterer = new Loadable<MethodDefinition>(CreateSimpleComponentRegisterer);
        InterfaceComponentRegisterer = new Loadable<MethodDefinition>(CreateInterfaceComponentRegisterer);
        ComponentRegistererMethod = new Loadable<MethodDefinition>(CreateComponentRegistererMethod);
    }

    private MethodDefinition CreateComponentRegistererMethod()
    {
        var method = new MethodDefinition(
            "RegisterCurrentPlugin",
            MethodAttributes.Static | MethodAttributes.Assembly,
            _context.ProcessingModule.TypeSystem.Void
        );

        _componentRegistererType.Value.Methods.Add(method);

        var il = method.Body.GetILProcessor();
        il.Emit(OpCodes.Ret);
        
        CallInTopOfEntryPoint(method);

        return method;
    }

    private MethodDefinition CreateInterfaceComponentRegisterer()
    {
        var method = new MethodDefinition(
            "RegisterSerializedComponent",
            MethodAttributes.Static | MethodAttributes.Private,
            _context.ProcessingModule.TypeSystem.Void
        );
        
        var genericParameter = new GenericParameter("T", method)
        {
            Attributes = GenericParameterAttributes.NonVariant
        };
        var monoBehaviourType = _context.ProcessingModule.ImportReference(_context.InteropTypes.MonoBehaviour.Value);
        genericParameter.Constraints.Add(new GenericParameterConstraint(monoBehaviourType));
        method.GenericParameters.Add(genericParameter);
        
        var registerer = new GenericInstanceMethod(
            _context.ProcessingModule.ImportReference(_context.InteropTypes.RegisterTypeInIl2CppWithOptionsMethod.Value));
        registerer.GenericArguments.Add(genericParameter);
        
        var registererOptionsConstructor = _context.ProcessingModule.ImportReference(
            _context.InteropTypes.ClassInjectorRegisterOptionsConstructor.Value);
        var registerOptionsSetInterfaceMethod = _context.ProcessingModule.ImportReference(
            _context.InteropTypes.Il2CppInterfaceCollectionSetInterfaceMethod.Value);
        var systemType = _context.ProcessingModule.ImportReference(_context.InteropTypes.SystemType.Value);
        var serializationInterface = _context.ProcessingModule.ImportReference(
            _context.InteropTypes.SerializationCallbackReceiverInterface.Value);
        var getTypeHandle = _context.ProcessingModule.ImportReference(
            _context.InteropTypes.GetSystemTypeFromHandleMethod.Value);
        var interfaceCollectionConstructor = _context.ProcessingModule.ImportReference(
            _context.InteropTypes.Il2CppInterfaceCollectionConstructor.Value);
        
        var var0 = new VariableDefinition(
            _context.ProcessingModule.ImportReference(registererOptionsConstructor.DeclaringType));
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
        
        _componentRegistererType.Value.Methods.Add(method);

        return method;
    }
    
    private MethodDefinition CreateSimpleComponentRegisterer()
    {
        var method = new MethodDefinition(
            "RegisterComponent",
            MethodAttributes.Static | MethodAttributes.Private,
            _context.ProcessingModule.TypeSystem.Void
        );

        var genericParameter = new GenericParameter("T", method)
        {
            Attributes = GenericParameterAttributes.NonVariant
        };
        var monoBehaviourType = _context.ProcessingModule.ImportReference(_context.InteropTypes.MonoBehaviour.Value);
        genericParameter.Constraints.Add(new GenericParameterConstraint(monoBehaviourType));
        method.GenericParameters.Add(genericParameter);

        var registerer = new GenericInstanceMethod(
            _context.ProcessingModule.ImportReference(_context.InteropTypes.SimpleRegisterTypeInIl2CppMethod.Value));
        registerer.GenericArguments.Add(genericParameter);


        var il = method.Body.GetILProcessor();
        il.Emit(OpCodes.Call, registerer);
        il.Emit(OpCodes.Ret);

        _componentRegistererType.Value.Methods.Add(method);

        return method;
    }
    
    private void CallInTopOfEntryPoint(MethodReference methodToCall)
    {
        var il = _pluginEntryPoint.Value.Body.GetILProcessor();
        il.Prepend(il.Create(OpCodes.Call, methodToCall));
    }

    private TypeDefinition CreateComponentRegistererType()
    {
        var type = new TypeDefinition(
            BaseRuntimeNamespace,
            "ComponentRegisterer",
            TypeAttributesUtility.Internal | TypeAttributesUtility.Static | TypeAttributes.Class,
            _context.ProcessingModule.TypeSystem.Object
        );

        _context.ProcessingModule.Types.Add(type);

        return type;
    }

    private MethodDefinition FindEntryPoint()
    {
        var entryPoint = _context.ProcessingModule
            .GetBepInExPluginEntryPointMethod(_context.InteropTypes);

        if (entryPoint == null)
        {
            throw new Exception(
                $"{_context.ProcessingModule.Name} doesn't contains an BepInEx Il2Cpp plugin entry point");
        }

        return entryPoint;
    }
}