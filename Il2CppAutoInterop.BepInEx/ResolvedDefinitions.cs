using Il2CppAutoInterop.Cecil;
using Il2CppAutoInterop.Cecil.Attributes;
using Il2CppAutoInterop.Cecil.Interfaces;
using Il2CppAutoInterop.Logging;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx;

public sealed class ResolvedDefinitions : BaseDefinitionResolver
{
    [CecilResolve("UnityEngine.MonoBehaviour", ResolverContext.Referenced)]
    internal TypeDefinition MonoBehaviour { get; private set; } = null!;

    [CecilResolve("UnityEngine.SerializeField", ResolverContext.Referenced)]
    internal TypeDefinition SerializeFieldAttribute { get; private set; } = null!;

    [CecilResolve("System.NonSerializedAttribute", ResolverContext.Referenceable)]
    internal TypeDefinition NonSerializedAttribute { get; private set; } = null!;

    [CecilResolve("BepInEx.Unity.IL2CPP.BasePlugin", ResolverContext.Referenced)]
    internal TypeDefinition BepInExBasePlugin { get; private set; } = null!;

    [CecilResolve("Il2CppInterop.Runtime.InteropTypes.Fields.Il2CppStringField", ResolverContext.Referenced)]
    internal TypeDefinition Il2CppStringField { get; private set; } = null!;

    [CecilResolve("Il2CppInterop.Runtime.InteropTypes.Fields.Il2CppReferenceField`1", ResolverContext.Referenced)]
    internal TypeDefinition Il2CppReferenceField { get; private set; } = null!;

    [CecilResolve("Il2CppInterop.Runtime.InteropTypes.Fields.Il2CppValueField`1", ResolverContext.Referenced)]
    internal TypeDefinition Il2CppValueField { get; private set; } = null!;

    [CecilResolve("UnityEngine.Object", ResolverContext.Referenced)]
    internal TypeDefinition UnityEngineObject { get; private set; } = null!;
    
    [CecilResolve("Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase", ResolverContext.Referenced)]
    internal TypeDefinition Il2CppObjectBase { get; private set; } = null!;
    
    [CecilResolve("System.Void Il2CppInterop.Runtime.Attributes.HideFromIl2CppAttribute::.ctor()", ResolverContext.Referenced)]
    internal MethodDefinition HideFromIl2CppAttributeConstructor { get; private set; } = null!;
    
    [CecilResolve("System.Void System.NotImplementedException::.ctor()", ResolverContext.Referenceable)]
    internal MethodDefinition NotImplementedExceptionConstructor { get; private set; } = null!;
    
    [CecilResolve("System.Void Il2CppInterop.Runtime.Injection.ClassInjector::RegisterTypeInIl2Cpp()", ResolverContext.Referenced)]
    internal MethodDefinition SimpleRegisterTypeInIl2CppMethod { get; private set; } = null!;
    
    [CecilResolve("System.Void Il2CppInterop.Runtime.Injection.ClassInjector::RegisterTypeInIl2Cpp(System.Type,Il2CppInterop.Runtime.Injection.RegisterTypeOptions)", ResolverContext.Referenced)]
    internal MethodDefinition RegisterTypeInIl2CppWithOptionsMethod { get; private set; } = null!;
    
    [CecilResolve("System.Void Il2CppInterop.Runtime.Injection.RegisterTypeOptions::.ctor()", ResolverContext.Referenced)]
    internal MethodDefinition ClassInjectorRegisterOptionsConstructor { get; set; } = null!;

    [CecilResolve("UnityEngine.ISerializationCallbackReceiver", ResolverContext.Referenced)]
    internal TypeDefinition SerializationCallbackReceiverInterface { get; private set; } = null!;
    
    [CecilResolve("System.Type", ResolverContext.Referenceable)]
    internal TypeDefinition SystemType { get; set; } = null!;
    
    [CecilResolve("System.Type System.Type::GetTypeFromHandle(System.RuntimeTypeHandle)", ResolverContext.Referenceable)]
    internal MethodDefinition GetSystemTypeFromHandleMethod { get; set; } = null!;
    
    [CecilResolve("System.Void modreq(System.Runtime.CompilerServices.IsExternalInit) Il2CppInterop.Runtime.Injection.RegisterTypeOptions::set_Interfaces(Il2CppInterop.Runtime.Injection.Il2CppInterfaceCollection)", ResolverContext.Referenced)]
    internal MethodDefinition Il2CppInterfaceCollectionSetInterfaceMethod { get; set; } = null!;
    
    [CecilResolve("System.Void Il2CppInterop.Runtime.Injection.Il2CppInterfaceCollection::.ctor(System.Collections.Generic.IEnumerable`1<System.Type>)", ResolverContext.Referenceable)]
    internal MethodDefinition Il2CppInterfaceCollectionConstructor { get; set; } = null!;
    
    [CecilResolve("System.Void <>z__ReadOnlySingleElementList`1::.ctor(T)")]
    internal MethodDefinition ReadOnlySingleElementListConstructor { get; private set; } = null!;

    internal ResolvedDefinitions(IAssemblyLoaderContext loader, ModuleDefinition mainModule) : base(loader, mainModule)
    {
        Logger.Instance.Warning(Il2CppInterfaceCollectionSetInterfaceMethod.FullName);
    }
}