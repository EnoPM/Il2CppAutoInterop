using Il2CppAutoInterop.Common.Interfaces;
using Mono.Cecil;

namespace Il2CppAutoInterop.Cecil.Utils;

public sealed class LoadableMethod(
    ILoadOnAccess<MethodDefinition>.LoaderDelegate loader,
    string fullName) : BaseLoadableDefinition<MethodDefinition>(loader, fullName);