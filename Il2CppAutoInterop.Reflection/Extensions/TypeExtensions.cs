using Il2CppAutoInterop.Reflection.Utils;

namespace Il2CppAutoInterop.Reflection.Extensions;

public static class TypeExtensions
{
    public static IEnumerable<Type> ToTopologicalSortedDependencies(this Dictionary<Type, IEnumerable<Type>> dependenciesMap)
    {
        var sorter = new TopologicalSorter(dependenciesMap);
        return sorter.Sort();
    }
}