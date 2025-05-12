namespace Il2CppAutoInterop.Core.Contexts;

public sealed class Naming
{
    public readonly string RuntimeNamespace;

    public Naming(AutoInteropModule module)
    {
        RuntimeNamespace = string.Join('.', nameof(Il2CppAutoInterop), module.Module.Name, "Runtime");
    }
}