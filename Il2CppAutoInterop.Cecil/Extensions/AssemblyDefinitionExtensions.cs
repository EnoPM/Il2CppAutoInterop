using Mono.Cecil;

namespace Il2CppAutoInterop.Cecil.Extensions;

public static class AssemblyDefinitionExtensions
{
    public static TypeDefinition? Resolve(this AssemblyDefinition assembly, string typeFullName)
    {
        return assembly.ResolveInAssembly(typeFullName) ?? assembly.ResolveInReferences(typeFullName);
    }
    
    public static TypeDefinition? ResolveInAssembly(this AssemblyDefinition assembly, string typeFullName)
    {
        foreach (var module in assembly.Modules)
        {
            var result = module.ResolveInModule(typeFullName);
            if (result != null) return result;
        }
        return null;
    }
    
    public static TypeDefinition? ResolveInReferences(this AssemblyDefinition assembly, string typeFullName)
    {
        foreach (var module in assembly.Modules)
        {
            var result = module.ResolveInReferences(typeFullName);
            if (result != null) return result;
        }
        
        return null;
    }
}