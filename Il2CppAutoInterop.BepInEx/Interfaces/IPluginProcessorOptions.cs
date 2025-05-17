namespace Il2CppAutoInterop.BepInEx.Interfaces;

public interface IPluginProcessorOptions
{
    public string? OutputDirectoryPath { get; }
    public string BepInExDirectoryPath { get; }
    public string? UnityProjectDirectoryPath { get; }
    
    public bool UseUnitySerializationInterface { get; }
}