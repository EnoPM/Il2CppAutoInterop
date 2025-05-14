using System.Reflection;
using Il2CppAutoInterop.Cecil.Attributes;
using Il2CppAutoInterop.Cecil.Extensions;
using Il2CppAutoInterop.Cecil.Interfaces;
using Il2CppAutoInterop.Cecil.Utils;
using Mono.Cecil;

namespace Il2CppAutoInterop.Cecil;

public abstract class BaseDefinitionResolver
{
    private readonly IAssemblyLoaderContext _loaderContext;
    private readonly ModuleDefinition _mainModuleDefinition;
    
    protected BaseDefinitionResolver(IAssemblyLoaderContext loaderContext, ModuleDefinition mainModule)
    {
        _loaderContext = loaderContext;
        _mainModuleDefinition = mainModule;
        
        HydrateProperties();
    }
    
    protected virtual TypeDefinition LoadTypeByFullName(string typeName, ResolverContext context = ResolverContext.All)
    {
        var type = context switch
        {
            ResolverContext.All => _mainModuleDefinition.Resolve(typeName, _loaderContext),
            ResolverContext.Internal => _mainModuleDefinition.ResolveInModule(typeName),
            ResolverContext.Referenced => _mainModuleDefinition.ResolveInReferences(typeName),
            ResolverContext.Unreferenced => !_loaderContext.TryResolveUnreferenced(_mainModuleDefinition, typeName, out var resolvedType) ? null : resolvedType,
            ResolverContext.Referenceable => _mainModuleDefinition.ResolveInReferences(typeName, _loaderContext),
            _ => throw new Exception($"Unknown resolver context {context}")
        };
        if (type == null)
        {
            throw new Exception($"Unable to resolve type {typeName} with context {context}");
        }

        return type;
    }

    protected virtual MethodDefinition LoadMethodByFullName(string methodFullName, ResolverContext context = ResolverContext.All)
    {
        var typeFullName = MethodsUtility.ParseTypeFullNameFromMethodFullName(methodFullName);
        var type = LoadTypeByFullName(typeFullName, context);
        var method = type.Methods.FirstOrDefault(x => x.FullName == methodFullName);
        if (method == null)
        {
            throw new Exception($"Unable to resolve method {methodFullName}");
        }

        return method;
    }
    
    private void HydrateProperties()
    {
        var properties = GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(p => p.GetCustomAttribute<CecilResolveAttribute>() != null)
            .ToDictionary(p => p, p => p.GetCustomAttribute<CecilResolveAttribute>()!);

        foreach (var (property, attribute) in properties)
        {
            if (property.PropertyType == typeof(TypeDefinition))
            {
                var type = LoadTypeByFullName(attribute.FullName, attribute.Context);
                property.SetValue(this, type);
            }
            else if (property.PropertyType == typeof(MethodDefinition))
            {
                var method = LoadMethodByFullName(attribute.FullName, attribute.Context);
                property.SetValue(this, method);
            }
        }
    }
}