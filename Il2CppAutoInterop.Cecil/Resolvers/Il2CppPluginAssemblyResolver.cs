using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Mono.Cecil;

namespace Il2CppAutoInterop.Cecil.Resolvers;

public class Il2CppPluginAssemblyResolver : DefaultAssemblyResolver
{
    private readonly ResolverContext _resolverContext;

    public Il2CppPluginAssemblyResolver(ResolverContext context)
    {
        _resolverContext = context;

        ResolveFailure += ResolveOnFailure;
    }

    private AssemblyDefinition? ResolveOnFailure(object sender, AssemblyNameReference reference)
    {
        if (!TryParseAssemblyName(reference.FullName, out var name))
        {
            Console.WriteLine($"Unable to resolve assembly name: {reference.FullName}");
            return null;
        }

        foreach (var dir in _resolverContext.SearchDirectories)
        {
            if (!Directory.Exists(dir))
            {
                Console.WriteLine($"Unable to resolve cecil search directory '{dir}'");
                continue;
            }

            if (TryResolveDllAssembly(name, dir, out var assembly))
            {
                return assembly;
            }
        }

        return null;
    }

    private static bool TryParseAssemblyName(string fullName, [MaybeNullWhen(false)] out AssemblyName assemblyName)
    {
        try
        {
            assemblyName = new AssemblyName(fullName);
            return true;
        }
        catch (Exception)
        {
            assemblyName = null;
            return false;
        }
    }

    private bool TryResolveDllAssembly(
        AssemblyName assemblyName,
        string directory,
        [MaybeNullWhen(false)] out AssemblyDefinition assembly) =>
        GenericResolver.TryResolveDllAssembly(
            assemblyName,
            directory,
            s => AssemblyDefinition.ReadAssembly(s, _resolverContext.Parameters),
            out assembly
        );
}