using System.Diagnostics.CodeAnalysis;
using Il2CppAutoInterop.Common.Interfaces;

namespace Il2CppAutoInterop.Common;

public class Loadable<T> : ILoadOnAccess<T>
where T : notnull
{
    public event ILoadOnAccess<T>.ItemLoadedDelegate? Loaded;
    public T Value
    {
        get
        {
            if (!IsLoaded)
            {
                Load();
            }
            if (HasValue)
            {
                return _value!;
            }
            throw new Exception($"[Lazy] Value {typeof(T).Name} has no value.");
        }
    }
    
    public bool IsLoaded { get; private set; }
    public bool HasValue { get; private set; }

    private readonly ILoadOnAccess<T>.LoaderDelegate _loader;
    
    private T? _value;

    public Loadable(ILoadOnAccess<T>.LoaderDelegate loader)
    {
        _loader = loader;
    }
    
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return HasValue;
    }
    
    public void Load()
    {
        if (IsLoaded) return;
        IsLoaded = true;
        _value = _loader();
        HasValue = true;
        TriggerItemLoadedEvent(_value);
    }
    
    private void TriggerItemLoadedEvent(T value)
    {
        Loaded?.Invoke(value);
    }
}