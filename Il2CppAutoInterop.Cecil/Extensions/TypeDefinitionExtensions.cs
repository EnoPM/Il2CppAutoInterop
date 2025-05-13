using System.Diagnostics.CodeAnalysis;
using Mono.Cecil;

namespace Il2CppAutoInterop.Cecil.Extensions;

public static class TypeDefinitionExtensions
{
    public static bool TryFindNestedType(
        this TypeDefinition type,
        string fullName,
        [MaybeNullWhen(false)] out TypeDefinition result)
    {
        if (type.FullName == fullName)
        {
            result = type;
            return true;
        }

        foreach (var nested in type.NestedTypes)
        {
            if (!nested.TryFindNestedType(fullName, out var nestedResult)) continue;
            result = nestedResult;
            return true;
        }
        result = null;
        return false;
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

    public static bool TryFindNearestMethod(
        this TypeDefinition type,
        Func<MethodDefinition, bool> methodFilter,
        [MaybeNullWhen(false)] out MethodDefinition method)
    {
        var baseType = type;

        while (baseType != null)
        {
            var nearestMethod = baseType.Methods.FirstOrDefault(methodFilter);

            if (nearestMethod != null)
            {
                method = nearestMethod;
                return true;
            }

            if (baseType.BaseType == null)
            {
                method = null;
                return false;
            }

            baseType = baseType.BaseType.Resolve();
        }

        method = null;
        return false;
    }
}