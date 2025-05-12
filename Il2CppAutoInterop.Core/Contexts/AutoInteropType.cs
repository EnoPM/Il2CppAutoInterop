using Il2CppAutoInterop.Core.Interfaces;
using Mono.Cecil;

namespace Il2CppAutoInterop.Core.Contexts;

public sealed class AutoInteropType : IAutoInteropContext
{
    public readonly TypeDefinition Type;

    public AutoInteropType(TypeDefinition type)
    {
        Type = type;
    }
}