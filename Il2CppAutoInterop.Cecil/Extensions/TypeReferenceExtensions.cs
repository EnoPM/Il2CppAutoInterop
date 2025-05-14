using Mono.Cecil;

namespace Il2CppAutoInterop.Cecil.Extensions;

public static class TypeReferenceExtensions
{
    public static GenericInstanceType CreateGenericInstanceType(this TypeReference type, params TypeReference[] typeArguments)
    {
        var genericType = new GenericInstanceType(type);
        foreach (var typeArgument in typeArguments)
        {
            genericType.GenericArguments.Add(typeArgument);
        }

        return genericType;
    }
}