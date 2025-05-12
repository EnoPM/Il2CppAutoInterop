using System.Reflection;
using Il2CppAutoInterop.Core.DependencyInjection.Exceptions;
using Il2CppAutoInterop.Reflection.Extensions;

namespace Il2CppAutoInterop.Core.DependencyInjection;

public sealed class DependencyInjector
{
    private readonly DependencyCollection _context;
    
    internal DependencyInjector(DependencyCollection context)
    {
        _context = context;
    }
    
    public T Instantiate<T>()
    {
        return (T)Instantiate(typeof(T));
    }
    
    public object Instantiate(Type type)
    {
        var constructor = GetInjectableConstructor(type);
        var parameterTypes = constructor.GetDependencies().ToArray();

        var arguments = new object[parameterTypes.Length];
        for (var i = 0; i < parameterTypes.Length; i++)
        {
            var dependency = _context.Get(parameterTypes[i]);
            arguments[i] = dependency ?? throw new DependencyInjectorException($"Could not find dependency for {parameterTypes[i].FullName}");
        }

        return constructor.Invoke(arguments);
    }

    public ConstructorInfo GetInjectableConstructor(Type type)
    {
        var constructors = type
            .GetConstructors()
            .OrderByDescending(x => x.GetParameters().Length);

        var constructor = constructors.FirstOrDefault(IsInjectable);

        if (constructor == null)
        {
            throw new DependencyInjectorException($"Cannot find injectable constructor for {type.Name}");
        }

        return constructor;
    }

    private bool IsInjectable(ConstructorInfo constructor)
    {
        return constructor.GetParameters().All(IsInjectable);
    }

    private bool IsInjectable(ParameterInfo parameter)
    {
        return IsInjectable(parameter.ParameterType);
    }

    private bool IsInjectable(Type type)
    {
        return typeof(DependencyCollection) == type || _context.Exists(type, true);
    }
}