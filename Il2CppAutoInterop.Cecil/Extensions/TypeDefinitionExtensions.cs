using Mono.Cecil;

namespace Il2CppAutoInterop.Cecil.Extensions;

public static class TypeDefinitionExtensions
{
    public static TypeDefinition? FindNestedType(this TypeDefinition type, string fullName)
    {
        if (type.FullName == fullName)
            return type;

        foreach (var nested in type.NestedTypes)
        {
            var result = nested.FindNestedType(fullName);
            if (result != null) return result;
        }
        return null;
    }

    public static bool IsAssignableFrom(this TypeDefinition source, TypeDefinition target) => target.IsAssignableTo(source);

    public static bool IsAssignableTo(this TypeDefinition source, TypeDefinition target)
    {
        while (source.BaseType != null && source.BaseType.FullName != target.FullName)
        {
            if (source.BaseType == null)
            {
                return false;
            }

            if (source.BaseType.FullName == target.FullName)
            {
                return true;
            }

            var ancestor = source.BaseType.Resolve();
            if (ancestor == null)
            {
                Console.WriteLine($"Unable to resolve ancestor type {source.FullName}");
                return false;
            }

            source = ancestor;
        }

        return source.BaseType != null && source.BaseType.FullName == target.FullName;
    }
    
    
}