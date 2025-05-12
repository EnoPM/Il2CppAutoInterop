using System.Diagnostics.CodeAnalysis;
using Il2CppAutoInterop.Cecil.Extensions;
using Mono.Cecil;

namespace Il2CppAutoInterop.Cecil.Resolvers;

public class ResolverContext
{
    public readonly ReaderParameters Parameters;
    public readonly List<string> SearchDirectories;

    public ResolverContext(List<string> searchDirectories)
    {
        SearchDirectories = searchDirectories;
        var assemblyResolver = new Il2CppPluginAssemblyResolver(this);
        Parameters = new ReaderParameters
        {
            ReadingMode = ReadingMode.Immediate,
            InMemory = true,
            AssemblyResolver = assemblyResolver,
        };
    }

    public bool TryResolveUnreferenced(
        ModuleDefinition module,
        string typeFullName,
        [MaybeNullWhen(false)] out TypeDefinition resolvedType
    )
    {
        var excludedFiles = module.AssemblyReferences
            .Select(x => module.AssemblyResolver.Resolve(x))
            .SelectMany(x => x.Modules)
            .Select(x => x.FileName)
            .ToList();

        return TryResolveUnreferenced(typeFullName, excludedFiles, out resolvedType);
    }
    
    private bool TryResolveUnreferenced(
        string typeFullName,
        List<string> excludedFiles,
        [MaybeNullWhen(false)] out TypeDefinition resolvedType
    )
    {
        foreach (var directoryPath in SearchDirectories)
        {
            var files = Directory.GetFiles(directoryPath, "*.dll", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (excludedFiles.Contains(file)) continue;
                try
                {
                    using var assembly = AssemblyDefinition.ReadAssembly(file, Parameters);
                    var type = assembly.Resolve(typeFullName);
                    if (type != null)
                    {
                        resolvedType = type;
                        return true;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        resolvedType = null;
        return false;
    }
}