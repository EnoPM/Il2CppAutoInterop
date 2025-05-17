namespace Il2CppAutoInterop.BepInEx.Interfaces;

public interface IBepInExIl2CppInteropOptions : IPluginProcessorOptions
{
    public IEnumerable<string> InputFilePaths { get; }
    
    public bool UseVersionRandomizer { get; }
}