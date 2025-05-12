using Mono.Cecil;

namespace Il2CppAutoInterop.Cecil.Utils;

public static class TypeAttributesUtility
{
    public const TypeAttributes Static = TypeAttributes.Abstract | TypeAttributes.Sealed;
    public const TypeAttributes Internal = TypeAttributes.NotPublic;
}