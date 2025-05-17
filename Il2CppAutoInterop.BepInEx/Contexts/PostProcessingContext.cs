using Il2CppAutoInterop.BepInEx.Contexts.Summaries;
using Il2CppAutoInterop.BepInEx.Interfaces;
using Il2CppAutoInterop.Cecil.Interfaces;
using Il2CppAutoInterop.Core.Interfaces;

namespace Il2CppAutoInterop.BepInEx.Contexts;

public class PostProcessingContext : IContext
{
    public HashSet<string> AssemblyFilePaths { get; }
    public IAssemblyLoader? Loader { get; set; }

    public string BepInExDirectoryPath { get; }

    public string? UnityProjectDirectoryPath { get; }

    public InteropSummary InteropSummary { get; }

    public bool UseUnitySerializationInterface { get; }

    private PostProcessingContext(
        HashSet<string> assemblyFilePaths,
        IAssemblyLoader? loader,
        string bepInExDirectoryPath,
        string? unityProjectDirectoryPath,
        InteropSummary? interopSummary,
        bool? useUnitySerializationInterface
    )
    {
        AssemblyFilePaths = assemblyFilePaths;
        Loader = loader;
        BepInExDirectoryPath = bepInExDirectoryPath;
        UnityProjectDirectoryPath = unityProjectDirectoryPath;
        InteropSummary = interopSummary ?? new InteropSummary();
        UseUnitySerializationInterface = useUnitySerializationInterface ?? false;
    }

    protected PostProcessingContext(PostProcessingContext context) : this(
        context.AssemblyFilePaths,
        context.Loader,
        context.BepInExDirectoryPath,
        context.UnityProjectDirectoryPath,
        context.InteropSummary,
        context.UseUnitySerializationInterface
        )
    {
        
    }

    public PostProcessingContext(IBepInExIl2CppInteropOptions options)
        : this([..options.InputFilePaths],
            null,
            options.BepInExDirectoryPath,
            options.UnityProjectDirectoryPath,
            null,
            options.UseUnitySerializationInterface
        )
    {
    }
}