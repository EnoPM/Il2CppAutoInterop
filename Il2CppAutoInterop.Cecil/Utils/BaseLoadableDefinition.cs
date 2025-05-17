using Il2CppAutoInterop.Common;
using Il2CppAutoInterop.Common.Interfaces;

namespace Il2CppAutoInterop.Cecil.Utils;

public abstract class BaseLoadableDefinition<T>(
    ILoadOnAccess<T>.LoaderDelegate loader,
    string fullName) : Loadable<T>(loader)
    where T : notnull
{
    public string FullName { get; } = fullName;

}