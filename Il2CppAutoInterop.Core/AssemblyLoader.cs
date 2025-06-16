using System.Diagnostics.CodeAnalysis;
using Il2CppAutoInterop.Cecil.Extensions;
using Il2CppAutoInterop.Cecil.Interfaces;
using Il2CppAutoInterop.Common.Logging;
using Mono.Cecil;

namespace Il2CppAutoInterop.Core;

public sealed class AssemblyLoader : IAssemblyLoader
{
    public IAssemblyDependencyManager Dependencies { get; }
    private readonly ReaderParameters _readerParameters;

    public AssemblyLoader(AssemblyLoader? parent = null)
    {
        Dependencies = new AssemblyDependencyManager(this);

        _readerParameters = parent?._readerParameters ?? new ReaderParameters
        {
            ReadingMode = ReadingMode.Immediate,
            InMemory = true,
            AssemblyResolver = new AssemblyResolver(this)
        };
    }

    public AssemblyDefinition? ResolveAssembly(AssemblyNameReference assemblyName)
    {
        if (!assemblyName.TryResolveAssemblyName(out var name))
        {
            Console.Error.WriteLine($"Unable to resolve assembly name {assemblyName.FullName}");
            return null;
        }

        return Dependencies.FindLoadedAssembly(name);
    }

    public AssemblyDefinition Load(string assemblyPath)
    {
        var dependency = Dependencies.Files.FirstOrDefault(x => x.Path == assemblyPath);
        if (dependency == null)
        {
            dependency = new DependencyFile(assemblyPath);
            Dependencies.Files.Add(dependency);
            return dependency.LoadedAssembly = AssemblyDefinition.ReadAssembly(assemblyPath, _readerParameters);
        }
        if (dependency.IsLoaded)
        {
            return dependency.LoadedAssembly!;
        }
        return AssemblyDefinition.ReadAssembly(assemblyPath, _readerParameters);
    }

    public AssemblyDefinition Load(Stream assemblyStream)
    {
        return AssemblyDefinition.ReadAssembly(assemblyStream, _readerParameters);
    }

    public void LoadDependencies() => Dependencies.LoadAllFiles();

    public bool TryResolveUnreferenced(
        ModuleDefinition module,
        string typeFullName,
        [NotNullWhen(true)] out TypeDefinition? resolvedType)
    {
        var excludedFiles = CompileExcludedFiles(module);

        return TryResolveUnreferenced(typeFullName, excludedFiles, out resolvedType);
    }

    private List<string> CompileExcludedFiles(ModuleDefinition module)
    {
        var excludedFiles = new List<string>();

        foreach (var reference in module.AssemblyReferences)
        {
            try
            {
                var resolved = module.AssemblyResolver.Resolve(reference);
                var fileNames = resolved.Modules.Select(x => x.FileName.ToString()).ToList();
                excludedFiles.AddRange(fileNames);
            }
            catch (Exception)
            {
                // ignored
            }
        }
        
        return excludedFiles;
    }

    private bool TryResolveUnreferenced(
        string typeFullName,
        List<string> excludedFiles,
        [NotNullWhen(true)] out TypeDefinition? resolvedType
    )
    {
        resolvedType = Dependencies.FindLoadedType(typeFullName, in excludedFiles);
        return resolvedType != null;
    }
}