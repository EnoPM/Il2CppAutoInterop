using Mono.Cecil;

namespace Il2CppAutoInterop.Cecil.Interfaces;

public interface IAssemblyDependencyManager
{
    public void AddFile(params string[] files);
    public void AddDirectory(params string[] directory);
    
    public void ProcessUnloadedDependenciesLoading();
    
    public AssemblyDefinition? FindLoadedAssembly(AssemblyNameReference assemblyName);
    public TypeDefinition? FindLoadedType(string typeFullName, in List<string> excludedFiles);
}