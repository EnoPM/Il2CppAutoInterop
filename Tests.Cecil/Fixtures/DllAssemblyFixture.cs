using Mono.Cecil;
using Tests.Cecil.Common;

namespace Tests.Cecil.Fixtures;

public class DllAssemblyFixture : IDisposable
{
    public AssemblyDefinition Assembly { get; }

    public DllAssemblyFixture(string assemblyName = "TestAssembly")
    {
        var name = new AssemblyNameDefinition(assemblyName, Utility.RandomVersion);
        Assembly = AssemblyDefinition.CreateAssembly(name, assemblyName, ModuleKind.Dll);
    }
    
    public TypeDefinition CreatePublicStaticClass(string name = Utility.TestClassName)
    {
        const TypeAttributes attributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed;
        var type = new TypeDefinition(
            Assembly.Name.Name,
            name,
            attributes,
            Assembly.MainModule.TypeSystem.Object);

        Assembly.MainModule.Types.Add(type);
        return type;
    }

    public static DllAssemblyFixture Load(string assemblyName = "TestAssembly")
    {
        var bytes = File.ReadAllBytes(Path.Combine(Utility.OutputDirectory, $"{assemblyName}.dll"));
        var stream = new MemoryStream(bytes);
        var assembly = AssemblyDefinition.ReadAssembly(stream, new ReaderParameters
        {
            ReadingMode = ReadingMode.Immediate
        });
        
        return new DllAssemblyFixture(assembly);
    }

    public string Save(string? outputPath = null)
    {
        outputPath ??= Path.Combine(Utility.OutputDirectory, $"{Assembly.Name.Name}.dll");
        Assembly.Write(outputPath);
        return outputPath;
    }
    
    public void Dispose()
    {
        Assembly.Dispose();
        GC.SuppressFinalize(this);
    }
    
    private DllAssemblyFixture(AssemblyDefinition assembly)
    {
        Assembly = assembly;
    }
}