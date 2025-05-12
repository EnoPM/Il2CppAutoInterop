using Il2CppAutoInterop.Core.Contexts;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Utils;

public sealed class ResolvableTypes
{
    private static class ResolvableTypeNames
    {
        internal const string MonoBehaviour = "UnityEngine.MonoBehaviour";
        internal const string SerializeFieldAttribute = "UnityEngine.SerializeField";
        internal const string NonSerializedAttribute = "System.NonSerializedAttribute";
    }

    private readonly AutoInteropModule _module;
    public readonly TypeDefinition MonoBehaviour;
    public readonly TypeDefinition SerializeFieldAttribute;
    public readonly TypeDefinition NonSerializedAttribute;
    
    public ResolvableTypes(AutoInteropModule module)
    {
        _module = module;
        MonoBehaviour = Type(ResolvableTypeNames.MonoBehaviour);
        SerializeFieldAttribute = Type(ResolvableTypeNames.SerializeFieldAttribute);
        NonSerializedAttribute = Type(ResolvableTypeNames.NonSerializedAttribute);
    }

    private TypeDefinition Type(string typeFullName)
    {
        return _module.Resolve(typeFullName);
    }
}