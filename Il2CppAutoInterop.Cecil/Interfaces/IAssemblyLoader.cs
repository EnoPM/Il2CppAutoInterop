using System.Diagnostics.CodeAnalysis;
using Mono.Cecil;

namespace Il2CppAutoInterop.Cecil.Interfaces;

public interface IAssemblyLoader
{
    public IAssemblyDependencyManager Dependencies { get; }
    
    public AssemblyDefinition? ResolveAssembly(AssemblyNameReference nameReference);
    public AssemblyDefinition Load(string assemblyPath);
    public AssemblyDefinition Load(Stream assemblyStream);

    public void LoadDependencies();

    public bool TryResolveUnreferenced(
        ModuleDefinition module,
        string typeFullName,
        [MaybeNullWhen(false)] out TypeDefinition resolvedType
    );
}