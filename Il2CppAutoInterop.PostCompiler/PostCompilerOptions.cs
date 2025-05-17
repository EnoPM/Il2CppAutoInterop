using CommandLine;
using Il2CppAutoInterop.BepInEx.Interfaces;

namespace Il2CppAutoInterop.PostCompiler;

public sealed class PostCompilerOptions : IBepInExIl2CppInteropOptions
{
    [Option('i', "input", Required = true, HelpText = "Input dll assemblies file paths to be processed.")]
    public IEnumerable<string> InputFilePaths { get; set; } = null!;

    [Option('o', "output", Required = false, HelpText = "Output directory path.")]
    public string? OutputDirectoryPath { get; set; }

    [Option('b', "BepInEx", Required = true, HelpText = "BepInEx directory with il2cpp interop generated")]
    public string BepInExDirectoryPath { get; set; } = null!;

    [Option('u', "unity-project", Required = false, HelpText = "Unity project directory path.")]
    public string? UnityProjectDirectoryPath { get; set; }

    [Option("experimental-serialization", Required = false, Default = false, HelpText = "Use experimental 'ISerializationCallbackReceiver.OnAfterDeserialize' method instead of 'MonoBehaviour.Awake'.")]
    public bool UseUnitySerializationInterface { get; set; }

    [Option( "randomize-version", Required = false, Default = false, HelpText = "Randomize post compiled assembly version")]
    public bool UseVersionRandomizer { get; set; } = false;
}