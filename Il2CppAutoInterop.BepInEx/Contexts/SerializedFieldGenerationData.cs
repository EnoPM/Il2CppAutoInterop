using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Contexts;

public sealed class SerializedFieldGenerationData
{
    public FieldDefinition UsableField { get; }
    public FieldDefinition SerializedField { get; }

    public SerializedFieldGenerationData(FieldDefinition usableField, FieldDefinition serializedField)
    {
        UsableField = usableField;
        SerializedField = serializedField;
    }
}