using Il2CppAutoInterop.Cecil.Interfaces;

namespace Il2CppAutoInterop.Core.Interfaces;

public interface IFileProcessorContext : IContext
{
    public string InputPath { get; }
    public IAssemblyLoader? Loader { get; set; }
}