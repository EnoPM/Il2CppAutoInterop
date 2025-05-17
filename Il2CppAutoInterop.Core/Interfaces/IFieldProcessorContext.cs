using Mono.Cecil;

namespace Il2CppAutoInterop.Core.Interfaces;

public interface IFieldProcessorContext : ITypeProcessorContext
{
    public FieldDefinition ProcessingField { get; }
}