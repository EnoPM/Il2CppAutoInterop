using System.Reflection;

namespace Il2CppAutoInterop.Reflection.Extensions;

public static class ConstructorInfoExtensions
{
    public static IEnumerable<Type> GetDependencies(this ConstructorInfo constructor)
    {
        var parameters = constructor.GetParameters();
        return parameters.Select(x => x.ParameterType);
    }
}