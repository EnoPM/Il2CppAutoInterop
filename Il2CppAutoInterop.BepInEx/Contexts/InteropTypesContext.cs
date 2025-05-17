using Il2CppAutoInterop.Cecil;
using Il2CppAutoInterop.Cecil.Attributes;
using Il2CppAutoInterop.Cecil.Interfaces;
using Il2CppAutoInterop.Cecil.Utils;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Contexts;

public sealed class InteropTypesContext : BaseDefinitionResolver
{
    [CecilResolve("UnityEngine.MonoBehaviour", ResolverContext.Referenceable)]
    internal LoadableType MonoBehaviour { get; private set; } = null!;

    [CecilResolve("UnityEngine.SerializeField", ResolverContext.Referenceable)]
    internal LoadableType SerializeFieldAttribute { get; private set; } = null!;

    [CecilResolve("System.NonSerializedAttribute", ResolverContext.Referenceable)]
    internal LoadableType NonSerializedAttribute { get; private set; } = null!;

    [CecilResolve("BepInEx.Unity.IL2CPP.BasePlugin", ResolverContext.Referenceable)]
    internal LoadableType BepInExBasePlugin { get; private set; } = null!;

    [CecilResolve("Il2CppInterop.Runtime.InteropTypes.Fields.Il2CppStringField", ResolverContext.Referenceable)]
    internal LoadableType Il2CppStringField { get; private set; } = null!;

    [CecilResolve("Il2CppInterop.Runtime.InteropTypes.Fields.Il2CppReferenceField`1", ResolverContext.Referenceable)]
    internal LoadableType Il2CppReferenceField { get; private set; } = null!;

    [CecilResolve("Il2CppInterop.Runtime.InteropTypes.Fields.Il2CppValueField`1", ResolverContext.Referenceable)]
    internal LoadableType Il2CppValueField { get; private set; } = null!;

    [CecilResolve("UnityEngine.Object", ResolverContext.Referenceable)]
    internal LoadableType UnityEngineObject { get; private set; } = null!;
    
    [CecilResolve("Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase", ResolverContext.Referenceable)]
    internal LoadableType Il2CppObjectBase { get; private set; } = null!;
    
    [CecilResolve("System.Void Il2CppInterop.Runtime.Attributes.HideFromIl2CppAttribute::.ctor()", ResolverContext.Referenceable)]
    internal LoadableMethod HideFromIl2CppAttributeConstructor { get; private set; } = null!;
    
    [CecilResolve("System.Void System.NotImplementedException::.ctor()", ResolverContext.Referenceable)]
    internal LoadableMethod NotImplementedExceptionConstructor { get; private set; } = null!;
    
    [CecilResolve("System.Void Il2CppInterop.Runtime.Injection.ClassInjector::RegisterTypeInIl2Cpp()", ResolverContext.Referenceable)]
    internal LoadableMethod SimpleRegisterTypeInIl2CppMethod { get; private set; } = null!;
    
    [CecilResolve("System.Void Il2CppInterop.Runtime.Injection.ClassInjector::RegisterTypeInIl2Cpp(Il2CppInterop.Runtime.Injection.RegisterTypeOptions)", ResolverContext.Referenceable)]
    internal LoadableMethod RegisterTypeInIl2CppWithOptionsMethod { get; private set; } = null!;
    
    [CecilResolve("Il2CppInterop.Runtime.Injection.RegisterTypeOptions", ResolverContext.Referenceable)]
    internal LoadableType ClassInjectorRegisterOptionsType { get; set; } = null!;
    
    [CecilResolve("System.Void Il2CppInterop.Runtime.Injection.RegisterTypeOptions::.ctor()", ResolverContext.Referenceable)]
    internal LoadableMethod ClassInjectorRegisterOptionsConstructor { get; set; } = null!;

    [CecilResolve("UnityEngine.ISerializationCallbackReceiver", ResolverContext.Referenceable)]
    internal LoadableType SerializationCallbackReceiverInterface { get; private set; } = null!;
    
    [CecilResolve("System.Type", ResolverContext.Referenceable)]
    internal LoadableType SystemType { get; set; } = null!;
    
    [CecilResolve("System.Type System.Type::GetTypeFromHandle(System.RuntimeTypeHandle)", ResolverContext.Referenceable)]
    internal LoadableMethod GetSystemTypeFromHandleMethod { get; set; } = null!;
    
    [CecilResolve("System.Void modreq(System.Runtime.CompilerServices.IsExternalInit) Il2CppInterop.Runtime.Injection.RegisterTypeOptions::set_Interfaces(Il2CppInterop.Runtime.Injection.Il2CppInterfaceCollection)", ResolverContext.Referenceable)]
    internal LoadableMethod Il2CppInterfaceCollectionSetInterfaceMethod { get; set; } = null!;
    
    [CecilResolve("System.Void Il2CppInterop.Runtime.Injection.Il2CppInterfaceCollection::.ctor(System.Collections.Generic.IEnumerable`1<System.Type>)", ResolverContext.Referenceable)]
    internal LoadableMethod Il2CppInterfaceCollectionConstructor { get; set; } = null!;

    internal InteropTypesContext(IAssemblyLoader loader, ModuleDefinition module) : base(loader, module)
    {
    }
}