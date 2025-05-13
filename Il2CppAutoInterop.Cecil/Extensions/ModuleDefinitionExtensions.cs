using Il2CppAutoInterop.Cecil.Interfaces;
using Mono.Cecil;

namespace Il2CppAutoInterop.Cecil.Extensions;

public static class ModuleDefinitionExtensions
{
    
    public static TypeDefinition? Resolve(
        this ModuleDefinition module,
        string typeFullName,
        IAssemblyLoaderContext? resolverContext = null
    )
    {
        return module.ResolveInModule(typeFullName) ?? module.ResolveInReferences(typeFullName, resolverContext);
    }

    public static TypeDefinition? ResolveInModule(this ModuleDefinition module, string typeFullName)
    {
        foreach (var type in module.Types)
        {
            var result = type.FindNestedType(typeFullName);
            if (result != null) return result;
        }

        return null;
    }

    public static TypeDefinition? ResolveInReferences(
        this ModuleDefinition module,
        string typeFullName,
        IAssemblyLoaderContext? resolverContext = null
    )
    {
        foreach (var reference in module.AssemblyReferences)
        {
            try
            {
                var referencedAssembly = module.AssemblyResolver.Resolve(reference);
                var type = referencedAssembly.ResolveInAssembly(typeFullName);
                if (type != null)
                {
                    return type;
                }
            }
            catch (AssemblyResolutionException)
            {
                // ignored
            }
        }

        if (resolverContext == null) return null;

        return !resolverContext.TryResolveUnreferenced(module, typeFullName, out var resolvedType) ? null : resolvedType;
    }
    
    public static List<TypeDefinition> GetAllTypes(this ModuleDefinition module)
    {
        var result = new List<TypeDefinition>();
        foreach (var type in module.Types)
        {
            result.Add(type);
            if (!type.HasNestedTypes) continue;
            result.AddRange(type.NestedTypes);
        }

        return result;
    }
}