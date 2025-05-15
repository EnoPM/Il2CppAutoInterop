namespace Il2CppAutoInterop.Core.Utils;

public delegate T DefinitionBuilder<out T>() where T : notnull;

public delegate void DefinitionCreatedHandler<in T>(T data);

public sealed class OptionalDefinition<T> where T : notnull
{
    public event DefinitionCreatedHandler<T>? Created;
    
    private readonly DefinitionBuilder<T> _builder;

    public OptionalDefinition(DefinitionBuilder<T> builder)
    {
        _builder = builder;
    }

    private T? _cache;

    private T CreateCache()
    {
        _cache = _builder();
        Created?.Invoke(_cache);
        return _cache;
    }

    public void Create()
    {
        if (_cache != null) return;
        CreateCache();
    }

    public T Definition => _cache ?? CreateCache();
}