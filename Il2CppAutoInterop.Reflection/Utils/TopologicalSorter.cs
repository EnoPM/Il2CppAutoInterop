namespace Il2CppAutoInterop.Reflection.Utils;

public sealed class TopologicalSorter(Dictionary<Type, IEnumerable<Type>> dependenciesMap)
{
    private readonly HashSet<Type> _visited = [];
    private readonly HashSet<Type> _visiting = [];
    private readonly List<Type> _sorted = [];

    public IEnumerable<Type> Sort()
    {
        foreach (var item in dependenciesMap.Keys)
        {
            if (!_visited.Contains(item))
            {
                Visit(item);
            }
        }
        return _sorted;
    }

    private void Visit(Type item, bool inner = false)
    {
        if (_visiting.Contains(item))
            throw new InvalidOperationException($"Circular dependency detected: {item.Name}");

        if (_visited.Contains(item)) return;
        _visiting.Add(item);

        foreach (var dep in dependenciesMap.GetValueOrDefault(item, []))
        {
            Visit(dep, true);
        }

        _visiting.Remove(item);
        _visited.Add(item);
        if (inner) return;
        _sorted.Add(item);
    }
}