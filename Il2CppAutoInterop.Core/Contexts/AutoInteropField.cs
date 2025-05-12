using Mono.Cecil;

namespace Il2CppAutoInterop.Core.Contexts;

public sealed class AutoInteropField
{
    public readonly FieldDefinition Field;

    public AutoInteropField(FieldDefinition field)
    {
        Field = field;
    }
}