using Il2CppAutoInterop.Cecil.Interfaces;
using Mono.Cecil;

namespace Il2CppAutoInterop.Core.Interfaces;

public interface IAssemblyProcessorContext : IFileProcessorContext
{
    public new IAssemblyLoader Loader { get; }
    public AssemblyDefinition ProcessingAssembly { get; }
}