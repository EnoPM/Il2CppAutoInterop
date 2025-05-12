using Mono.Cecil;

namespace Il2CppAutoInterop.Cecil.Extensions;

public static class MemberDefinitionExtensions
{
    public static bool HasCustomAttribute(this IMemberDefinition member, TypeDefinition customAttributeType)
    {
        return member.CustomAttributes.Any(a => a.AttributeType.FullName == customAttributeType.FullName);
    }
}