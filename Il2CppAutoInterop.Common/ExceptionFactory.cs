using Il2CppAutoInterop.Common.Logging;

namespace Il2CppAutoInterop.Common;

public static class ExceptionFactory
{
    private static string AssemblyNotYetLoadedMessage(string assemblyIdentity)
    {
        return $"Assembly '{assemblyIdentity}' is not loaded";
    }

    private static string Source<T>(string source)
    {
        return $"[<{typeof(T).Name}>.{source}]";
    }
    
    public static Exception AssemblyNotYetLoaded<T>(string source, string assemblyIdentity)
    {
        return new Exception($"{Source<T>(source)} => {AssemblyNotYetLoadedMessage(assemblyIdentity)}");
    }
    
    public static void AssemblyNotYetLoadedError<T>(string source, string assemblyIdentity)
    {
        Logger.Instance.Error($"{Source<T>(source)} => {AssemblyNotYetLoadedMessage(assemblyIdentity)}");
    }
    
    public static void AssemblyNotYetLoadedError<T>(string source, string assemblyIdentity, Exception ex)
    {
        Logger.Instance.Error($"{Source<T>(source)} => {AssemblyNotYetLoadedMessage(assemblyIdentity)}: {ex.Message}");
    }
}