using System.Reflection;
using Il2CppAutoInterop.Cecil.Attributes;
using Il2CppAutoInterop.Cecil.Extensions;
using Il2CppAutoInterop.Cecil.Interfaces;
using Il2CppAutoInterop.Cecil.Utils;
using Il2CppAutoInterop.Common;
using Il2CppAutoInterop.Common.Interfaces;
using Mono.Cecil;

namespace Il2CppAutoInterop.Cecil;

public abstract class BaseDefinitionResolver
{
    private readonly IAssemblyLoader _loader;
    private readonly ModuleDefinition _module;

    protected BaseDefinitionResolver(IAssemblyLoader loader, ModuleDefinition module)
    {
        _loader = loader;
        _module = module;

        HydrateProperties();
    }
    
    private void HydrateProperties()
    {
        var properties = DiscoverResolvableProperties();

        foreach (var (property, attribute) in properties)
        {
            HydrateProperty(property, attribute);
        }
    }
    
    private MethodDefinition LoadMethodByFullName(string methodFullName, ResolverContext context = ResolverContext.All)
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

    private TypeDefinition LoadTypeByFullName(string typeName, ResolverContext context = ResolverContext.All)
    {
        var type = context switch
        {
            ResolverContext.All => _module.Resolve(typeName, _loader),
            ResolverContext.Internal => _module.ResolveInModule(typeName),
            ResolverContext.Referenced => _module.ResolveInReferences(typeName),
            ResolverContext.Unreferenced => !_loader.TryResolveUnreferenced(_module, typeName, out var resolvedType) ? null : resolvedType,
            ResolverContext.Referenceable => _module.ResolveInReferences(typeName, _loader),
            _ => throw new Exception($"Unknown resolver context {context}")
        };
        if (type == null)
        {
            throw new Exception($"Unable to resolve type {typeName} with context {context}");
        }

        return type;
    }

    private Dictionary<PropertyInfo, CecilResolveAttribute> DiscoverResolvableProperties()
    {
        var result = new Dictionary<PropertyInfo, CecilResolveAttribute>();

        var properties = GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var property in properties)
        {
            var attribute = property.GetCustomAttribute<CecilResolveAttribute>();
            if (attribute == null) continue;
            result.Add(property, attribute);
        }

        return result;
    }

    private void HydrateProperty(PropertyInfo property, CecilResolveAttribute attribute)
    {
        if (property.PropertyType == typeof(LoadableType))
        {
            var loader = MakeTypeDefinitionLoader(attribute);
            var loadableType = new LoadableType(loader, attribute.FullName);
            property.SetValue(this, loadableType);
        }
        else if (property.PropertyType == typeof(LoadableMethod))
        {
            var loader = MakeMethodDefinitionLoader(attribute);
            var loadableMethod = new LoadableMethod(loader, attribute.FullName);
            property.SetValue(this, loadableMethod);
        } else if (property.PropertyType == typeof(TypeDefinition))
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

    private ILoadOnAccess<TypeDefinition>.LoaderDelegate MakeTypeDefinitionLoader(
        CecilResolveAttribute attribute
    )

    {
        return () => LoadTypeByFullName(attribute.FullName, attribute.Context);
    }
    
    private ILoadOnAccess<MethodDefinition>.LoaderDelegate MakeMethodDefinitionLoader(
        CecilResolveAttribute attribute
    )

    {
        return () => LoadMethodByFullName(attribute.FullName, attribute.Context);
    }
}