namespace Tests.Cecil.Common;

internal static class Utility
{
    public const string OutputDirectory = @"D:\Projects\Il2CppAutoInterop\Tests.Cecil\Output";
    
    public const string LibraryAssemblyName = "TestLibrary";
    public const string PluginAssemblyName = "TestPlugin";
    
    public const string TestAssemblyName = "TestAssembly";
    public const string TestClassName = "TestClass";
    public const string TestMethodName = "TestMethod";
    public const string TestFieldName = "TestField";
    
    private static readonly Random Random = new();

    public static Version RandomVersion => new(
        Random.Next(0, 99),
        Random.Next(0, 999),
        Random.Next(0, 999),
        Random.Next(0, 999));
    
    
}