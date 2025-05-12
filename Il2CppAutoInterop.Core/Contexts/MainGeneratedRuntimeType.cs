using Il2CppAutoInterop.Cecil.Utils;
using Mono.Cecil;

namespace Il2CppAutoInterop.Core.Contexts;

public sealed class MainGeneratedRuntimeType
{
    public readonly TypeDefinition Type;

    public MainGeneratedRuntimeType(Naming naming, AutoInteropModule module)
    {
        Type = new TypeDefinition(
            naming.RuntimeNamespace,
            "AutoInterop",
            TypeAttributesUtility.Internal | TypeAttributesUtility.Static | TypeAttributes.Class,
            module.Module.TypeSystem.Object
        );
    }
}