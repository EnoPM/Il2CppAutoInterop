using Il2CppAutoInterop.Common.Interfaces;
using Mono.Cecil;

namespace Il2CppAutoInterop.Cecil.Utils;

public sealed class LoadableType(
    ILoadOnAccess<TypeDefinition>.LoaderDelegate loader,
    string fullName) : BaseLoadableDefinition<TypeDefinition>(loader, fullName);