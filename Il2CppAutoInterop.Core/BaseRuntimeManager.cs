namespace Il2CppAutoInterop.Core;

public abstract class BaseRuntimeManager
{
    protected readonly string BaseRuntimeNamespace;

    protected BaseRuntimeManager(string moduleName)
    {
        var parsedModuleName = ParseModuleName(moduleName);
        
        BaseRuntimeNamespace = Namespace(
            parsedModuleName,
            nameof(Il2CppAutoInterop),
            GetType().Name
        );
    }

    private static string ParseModuleName(string moduleName)
    {
        return Path.GetFileNameWithoutExtension(moduleName);
    }

    private static string Namespace(params string[] fragments)
    {
        return string.Join('.', fragments);
    }
}