using Mono.Cecil;
using Tests.Cecil.Common;

namespace Tests.Cecil.Factories;

internal static class AssemblyFactory
{
    public static AssemblyDefinition CreateDll(string name)
    {
        var assemblyName = new AssemblyNameDefinition(name, Utility.RandomVersion);
        var assembly = AssemblyDefinition.CreateAssembly(assemblyName, name, ModuleKind.Dll);

        return assembly;
    }

    public static AssemblyDefinition LoadInMemory(string name)
    {
        var bytes = File.ReadAllBytes(Path.Combine(Utility.OutputDirectory, $"{name}.dll"));
        var stream = new MemoryStream(bytes);
        return AssemblyDefinition.ReadAssembly(stream, new ReaderParameters
        {
            ReadingMode = ReadingMode.Immediate
        });
    }

    public static void Save(this AssemblyDefinition assembly, string? outputPath = null)
    {
        outputPath ??= assembly.ToLocationFilePath();
        assembly.Write(outputPath);
    }

    public static string ToLocationFilePath(this AssemblyDefinition assembly)
    {
        return Path.Combine(Utility.OutputDirectory, $"{assembly.Name.Name}.dll");
    }
}