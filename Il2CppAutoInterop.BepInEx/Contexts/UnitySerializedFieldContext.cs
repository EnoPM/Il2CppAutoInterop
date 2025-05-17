using Il2CppAutoInterop.Core.Interfaces;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Contexts;

public class UnitySerializedFieldContext : BepInExPluginSerializationContext, IFieldProcessorContext
{
    public FieldDefinition ProcessingField { get; }

    public UnitySerializedFieldContext(BepInExPluginSerializationContext context, FieldDefinition processingField) : base(context)
    {
        ProcessingField = processingField;
    }

    protected UnitySerializedFieldContext(UnitySerializedFieldContext context) : this(context, context.ProcessingField)
    {
    }
}