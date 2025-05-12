using Il2CppAutoInterop.Core.DependencyInjection.Exceptions;
using Il2CppAutoInterop.Reflection.Extensions;

namespace Il2CppAutoInterop.Core.DependencyInjection;

public class DependencyCollection
{
    private readonly List<DependencyCollection> _parents;
    private readonly DependencyInjector _injector;
    private readonly Dictionary<Type, object> _instances = [];
    private readonly List<Type> _candidates = [];

    public DependencyCollection(params DependencyCollection[] parents)
    {
        _parents = parents.ToList();
        _injector = new DependencyInjector(this);
    }

    private DependencyCollection(List<Type> candidates, DependencyCollection from)
    {
        _candidates = candidates;
        _parents = [from];
        _injector = new DependencyInjector(this);
    }

    public DependencyCollection CreateScope()
    {
        return new DependencyCollection(_candidates.ToList(), this);
    }

    public void RegisterParent(DependencyCollection parent)
    {
        if (ContainsContext(parent))
        {
            throw Exception("Trying to register parent twice");
        }
        _parents.Add(parent);
    }

    private bool ContainsContext(DependencyCollection collection)
    {
        return _instances.Values.Any(x => x == collection) || _parents.Any(x => x.ContainsContext(collection));
    }

    public void ProcessRegistration()
    {
        var types = FindDependencies(_candidates)
            .ToTopologicalSortedDependencies();

        foreach (var type in types)
        {
            RegisterImmediately(type, _injector.Instantiate(type));
        }

        _candidates.Clear();
    }

    public T RegisterImmediately<T>() where T : notnull
    {
        return RegisterImmediately(_injector.Instantiate<T>());
    }

    public void Register<T>()
    {
        Register(typeof(T));
    }

    public void Register(Type type)
    {
        if (_instances.ContainsKey(type))
        {
            throw Exception($"{type.FullName} is already registered");
        }
        if (_candidates.Contains(type))
        {
            return;
        }
        _candidates.Add(type);
    }

    public T RegisterImmediately<T>(T instance)
    {
        return (T)RegisterImmediately(typeof(T), instance!);
    }

    public object RegisterImmediately(Type type, object instance)
    {
        if (!_instances.TryAdd(type, instance))
        {
            throw Exception($"{type.FullName} is already registered");
        }
        return instance;
    }

    public T GetRequired<T>()
    {
        return (T)GetRequired(typeof(T));
    }

    public object GetRequired(Type type)
    {
        var instance = Get(type);
        if (instance == null)
        {
            throw Exception($"The type {type.Name} is not registered in {nameof(DependencyCollection)}");
        }

        return instance;
    }

    public T? Get<T>()
    {
        return (T?)Get(typeof(T));
    }

    public object? Get(Type type)
    {
        if (typeof(DependencyCollection) == type)
        {
            return this;
        }
        foreach (var instance in _instances)
        {
            if (!TypeComparator(instance.Key, type)) continue;
            return instance.Value;
        }
        return GetInParents(type);
    }

    public bool Exists(Type type, bool checkCandidates)
    {
        if (_instances.Any(instance => TypeComparator(instance.Key, type)))
        {
            return true;
        }
        if (checkCandidates)
        {
            if (_candidates.Any(candidate => TypeComparator(candidate, type)))
            {
                return true;
            }
        }
        return ExistsInParents(type, checkCandidates);
    }

    public IEnumerable<T> GetAll<T>()
    {
        return GetAll(typeof(T)).Cast<T>();
    }

    public IEnumerable<object> GetAll(Type type)
    {
        var result = new List<object>();
        result.AddRange(
            _instances
                .Where(x => TypeComparator(x.Key, type))
                .Select(x => x.Value)
        );
        result.AddRange(GetAllInParents(type));

        return result;
    }

    private static bool TypeComparator(Type instanceType, Type searchedType)
    {
        return instanceType.IsAssignableTo(searchedType);
    }

    private object? GetInParents(Type type)
    {
        foreach (var collection in _parents)
        {
            var result = collection.Get(type);
            if (result == null) continue;
            return result;
        }
        return null;
    }

    private bool ExistsInParents(Type type, bool checkCandidates)
    {
        return _parents.Any(collection => collection.Exists(type, checkCandidates));
    }

    private IEnumerable<object> GetAllInParents(Type type)
    {
        return _parents
            .SelectMany(collection => collection.GetAll(type));
    }

    private static Exception Exception(string message)
    {
        return new DependencyCollectionException(message);
    }

    private Dictionary<Type, IEnumerable<Type>> FindDependencies(IEnumerable<Type> types)
    {
        var results = new Dictionary<Type, IEnumerable<Type>>();
        foreach (var type in types)
        {
            var constructor = _injector.GetInjectableConstructor(type);
            var dependencies = constructor.GetDependencies();
            results.Add(type, dependencies);
        }

        return results;
    }

    private void LogDependencies()
    {
        foreach (var entry in _instances)
        {
            Console.WriteLine($"Collection item: {entry.Key.FullName}");
        }
    }
}