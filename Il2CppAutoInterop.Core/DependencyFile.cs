using Il2CppAutoInterop.Cecil.Interfaces;
using Mono.Cecil;

namespace Il2CppAutoInterop.Core;

public sealed class DependencyFile : IDependencyFile
{
    public string Path { get; }
    
    public bool CanBeLoaded { get; private set; }
    public AssemblyDefinition? LoadedAssembly { get; private set; }
    public bool IsLoaded => LoadedAssembly != null;
    public bool IsAvailable => CanBeLoaded && IsLoaded;
    
    internal DependencyFile(string filePath)
    {
        Path = filePath;
        CanBeLoaded = true;
    }

    public void Load(IAssemblyLoader loader)
    {
        try
        {
            LoadedAssembly = loader.Load(Path);
        }
        catch (BadImageFormatException)
        {
            CanBeLoaded = false;
        }
    }
}