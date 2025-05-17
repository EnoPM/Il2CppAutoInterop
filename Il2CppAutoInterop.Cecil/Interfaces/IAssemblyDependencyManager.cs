using System.Reflection;
using Mono.Cecil;

namespace Il2CppAutoInterop.Cecil.Interfaces;

public interface IAssemblyDependencyManager
{
    public IList<IDependencyFile> Files { get; }
    
    public void AddFile(params string[] files);
    public void AddDirectory(params string[] directory);
    
    public void LoadAllFiles();
    
    public AssemblyDefinition? FindLoadedAssembly(AssemblyName assemblyName);
    public TypeDefinition? FindLoadedType(string typeFullName, in List<string> excludedFiles);
}