using Il2CppAutoInterop.Cecil.Extensions;
using Il2CppAutoInterop.Cecil.Resolvers;
using Il2CppAutoInterop.Core.Exceptions;
using Il2CppAutoInterop.Core.Interfaces;
using Mono.Cecil;

namespace Il2CppAutoInterop.Core.Contexts;

public sealed class AutoInteropModule : IAutoInteropContext
{
    public readonly ModuleDefinition Module;
    private readonly ResolverContext _resolverContext;

    public AutoInteropModule(ModuleDefinition module, ResolverContext resolverContext)
    {
        Module = module;
        _resolverContext = resolverContext;
    }

    public TypeDefinition Resolve(string typeFullName)
    {
        var type = Module.Resolve(typeFullName, _resolverContext);
        if (type == null)
        {
            throw new AutoInteropModuleException($"Could not resolve type {typeFullName} from module {Module.Name}");
        }

        return type;
    }

    public TypeDefinition ResolveInReferences(string typeFullName)
    {
        var type = Module.ResolveInReferences(typeFullName, _resolverContext);
        if (type == null)
        {
            Console.WriteLine("throw");
            throw new AutoInteropModuleException($"Could not resolve type {typeFullName} from module {Module.Name} references");
        }

        return type;
    }
    
    public TypeDefinition ResolveInModule(string typeFullName)
    {
        var type = Module.ResolveInModule(typeFullName);
        if (type == null)
        {
            throw new AutoInteropModuleException($"Could not resolve type {typeFullName} from module {Module.Name}");
        }

        return type;
    }
}