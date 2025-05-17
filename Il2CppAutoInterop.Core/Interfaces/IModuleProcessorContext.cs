using Mono.Cecil;

namespace Il2CppAutoInterop.Core.Interfaces;

public interface IModuleProcessorContext : IAssemblyProcessorContext
{
    public ModuleDefinition ProcessingModule { get; }
}