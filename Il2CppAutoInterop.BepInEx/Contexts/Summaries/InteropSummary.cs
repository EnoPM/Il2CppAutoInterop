namespace Il2CppAutoInterop.BepInEx.Contexts.Summaries;

public sealed class InteropSummary
{
    public HashSet<string> SerializedMonoBehaviourFullNames { get; } = [];
    public HashSet<string> UnityProjectGeneratedFilePaths { get; } = [];
}