using System.Diagnostics.CodeAnalysis;

namespace Il2CppAutoInterop.Common.Interfaces;

public interface ILoadOnAccess<TItem>
{
    public delegate TItem LoaderDelegate();

    public delegate void ItemLoadedDelegate(TItem item);

    public event ItemLoadedDelegate? Loaded;

    public TItem Value { get; }

    public bool IsLoaded { get; }

    public bool TryGetValue([MaybeNullWhen(false)] out TItem value);
}