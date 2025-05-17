using Mono.Cecil;

namespace Il2CppAutoInterop.Core.Interfaces;

public interface ITypeProcessorContext : IModuleProcessorContext
{
    public TypeDefinition ProcessingType { get; }
}