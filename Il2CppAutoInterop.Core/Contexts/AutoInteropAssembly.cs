using Il2CppAutoInterop.Cecil.Resolvers;
using Il2CppAutoInterop.Core.Interfaces;
using Mono.Cecil;

namespace Il2CppAutoInterop.Core.Contexts;

public sealed class AutoInteropAssembly : IAutoInteropContext
{
    public readonly AssemblyDefinition Assembly;
    public readonly List<AutoInteropModule> Modules;

    public AutoInteropAssembly(AssemblyDefinition assembly, ResolverContext resolverContext)
    {
        Assembly = assembly;
        Modules = new List<AutoInteropModule>(assembly.Modules.Select(x => new AutoInteropModule(x, resolverContext)));
    }
}