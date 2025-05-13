namespace Il2CppAutoInterop.Core.Utils;

public sealed class TopologicalSorter<T> where T : notnull
{
    private readonly HashSet<T> _visited = [];
    private readonly HashSet<T> _visiting = [];
    private readonly List<T> _sorted = [];
    private readonly Dictionary<T, List<T>> _dependenciesMap = [];

    public delegate List<T> DependenciesResolver(T item);
    
    public TopologicalSorter(List<T> entries, DependenciesResolver dependenciesResolver)
    {
        foreach (var entry in entries)
        {
            _dependenciesMap[entry] = dependenciesResolver(entry)
                .Where(entries.Contains)
                .ToList();
        }
    }

    public List<T> Sort()
    {
        foreach (var item in _dependenciesMap.Keys)
        {
            if (!_visited.Contains(item))
            {
                Visit(item);
            }
        }
        return _sorted;
    }

    private void AddSorted(T item)
    {
        if (_sorted.Contains(item)) return;
        if (!_dependenciesMap.ContainsKey(item)) return;
        _sorted.Add(item);
    }

    private void Visit(T item)
    {
        if (_visiting.Contains(item))
            throw new InvalidOperationException($"Circular dependency detected: {item}");

        if (_visited.Contains(item)) return;
        _visiting.Add(item);

        foreach (var dep in _dependenciesMap.GetValueOrDefault(item, []))
        {
            Visit(dep);
        }

        _visiting.Remove(item);
        _visited.Add(item);
        AddSorted(item);
    }
}