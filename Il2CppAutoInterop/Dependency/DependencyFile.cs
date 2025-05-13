using System.Diagnostics.CodeAnalysis;
using Mono.Cecil;

namespace Il2CppAutoInterop.Dependency;

public sealed class DependencyFile
{
    public readonly AssemblyLoader Loader;
    public readonly string Path;
    
    public bool CanBeLoaded { get; private set; }
    public AssemblyDefinition? LoadedAssembly { get; private set; }
    public bool IsLoaded => LoadedAssembly != null;
    public bool IsAvailable => CanBeLoaded && IsLoaded;
    
    internal DependencyFile(AssemblyLoader loader, string filePath)
    {
        Loader = loader;
        Path = filePath;
        CanBeLoaded = true;
    }

    public void Load()
    {
        try
        {
            LoadedAssembly = Loader.Load(Path);
        }
        catch (BadImageFormatException)
        {
            CanBeLoaded = false;
        }
    }
}