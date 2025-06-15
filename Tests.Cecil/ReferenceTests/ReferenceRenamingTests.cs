using Mono.Cecil;
using Mono.Cecil.Cil;
using Tests.Cecil.Common;
using Tests.Cecil.Factories;
using Tests.Cecil.Fixtures;
using Xunit.Abstractions;

namespace Tests.Cecil.ReferenceTests;

public class ReferenceRenamingTests
{
    private readonly ITestOutputHelper _output;

    public ReferenceRenamingTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void CreateAssembliesWithMethodReferences()
    {
        DeleteAssembly(Utility.LibraryAssemblyName, Utility.PluginAssemblyName);

        var libraryMethod = CreateMethod(Utility.LibraryAssemblyName);
        var pluginMethod = CreateMethod(Utility.PluginAssemblyName);

        var il = pluginMethod.Body.GetILProcessor();
        var firstInstruction = pluginMethod.Body.Instructions.First();

        var importedMethod = pluginMethod.Module.ImportReference(libraryMethod);
        var callInstruction = il.Create(OpCodes.Call, importedMethod);

        il.InsertBefore(firstInstruction, callInstruction);

        libraryMethod.Module.Assembly.Save();
        pluginMethod.Module.Assembly.Save();
        
        Assert.True(HasMethodCall(pluginMethod, libraryMethod.FullName));

        Assert.True(File.Exists(libraryMethod.Module.Assembly.ToLocationFilePath()));
        Assert.True(File.Exists(pluginMethod.Module.Assembly.ToLocationFilePath()));
        
        libraryMethod.Module.Assembly.Dispose();
        pluginMethod.Module.Assembly.Dispose();
    }
    
    [Fact]
    public void CreateAssembliesWithFieldReferences()
    {
        DeleteAssembly(Utility.LibraryAssemblyName, Utility.PluginAssemblyName);

        var libraryField = CreateField(Utility.LibraryAssemblyName);
        var pluginField = CreateField(Utility.PluginAssemblyName);

        var pluginMethod = pluginField.DeclaringType.CreatePublicStaticVoidMethod();
        var il = pluginMethod.Body.GetILProcessor();
        var firstInstruction = pluginMethod.Body.Instructions.First();
        var importedLibraryField = pluginMethod.Module.ImportReference(libraryField);
        var importedPluginField = pluginMethod.Module.ImportReference(pluginField);
        var loadFieldInstruction = il.Create(OpCodes.Ldsfld, importedLibraryField);
        var storeFieldInstruction = il.Create(OpCodes.Stsfld, importedPluginField);
        il.InsertBefore(firstInstruction, loadFieldInstruction);
        il.InsertBefore(firstInstruction, storeFieldInstruction);

        libraryField.Module.Assembly.Save();
        pluginField.Module.Assembly.Save();
        
        Assert.True(HasFieldUsage(pluginMethod, libraryField.FullName));

        Assert.True(File.Exists(libraryField.Module.Assembly.ToLocationFilePath()));
        Assert.True(File.Exists(pluginField.Module.Assembly.ToLocationFilePath()));
        
        libraryField.Module.Assembly.Dispose();
        pluginField.Module.Assembly.Dispose();
    }

    [Fact]
    public void RenameMethodUsedInAnotherAssembly()
    {
        CreateAssembliesWithMethodReferences();

        var libraryMethod = LoadMethod(Utility.LibraryAssemblyName);
        var pluginMethod = LoadMethod(Utility.PluginAssemblyName);
        
        var oldMethodName = libraryMethod.FullName;
        libraryMethod.Name = $"{libraryMethod.Name}_Renamed";
        var newMethodName = libraryMethod.FullName;
        
        Assert.True(HasMethodCall(pluginMethod, oldMethodName));

        var callInstruction = pluginMethod.Body.Instructions
            .FirstOrDefault(x => x.OpCode == OpCodes.Call
                                 && x.Operand is MethodReference methodReference
                                 && methodReference.FullName == oldMethodName);
        Assert.NotNull(callInstruction);

        var il = pluginMethod.Body.GetILProcessor();
        
        var newCallInstruction = il.Create(
            callInstruction.OpCode,
            pluginMethod.Module.ImportReference(libraryMethod)
        );
        
        il.Replace(callInstruction, newCallInstruction);
        
        libraryMethod.Module.Assembly.Save();
        pluginMethod.Module.Assembly.Save();
        
        libraryMethod.Module.Assembly.Dispose();
        pluginMethod.Module.Assembly.Dispose();
        
        var loaded = LoadMethod(Utility.PluginAssemblyName);
        Assert.True(HasMethodCall(loaded, newMethodName));
    }
    
    [Fact]
    public void RenameFieldUsedInAnotherAssembly()
    {
        CreateAssembliesWithFieldReferences();

        var libraryField = LoadField(Utility.LibraryAssemblyName);
        var pluginField = LoadField(Utility.PluginAssemblyName);

        var pluginMethod = pluginField.DeclaringType.Methods
            .FirstOrDefault(x => x.Name == Utility.TestMethodName);
        Assert.NotNull(pluginMethod);
        
        var oldFieldName = libraryField.FullName;
        libraryField.Name = $"{libraryField.Name}_Renamed";
        var newFieldName = libraryField.FullName;
        
        Assert.True(HasFieldUsage(pluginMethod, oldFieldName));

        var callInstruction = pluginMethod.Body.Instructions
            .FirstOrDefault(x => FieldOpCodes.Contains(x.OpCode)
                                 && x.Operand is FieldReference fieldReference
                                 && fieldReference.FullName == oldFieldName);
        Assert.NotNull(callInstruction);

        var il = pluginMethod.Body.GetILProcessor();
        
        var newCallInstruction = il.Create(
            callInstruction.OpCode,
            pluginMethod.Module.ImportReference(libraryField)
        );
        
        il.Replace(callInstruction, newCallInstruction);
        
        libraryField.Module.Assembly.Save();
        pluginField.Module.Assembly.Save();
        
        libraryField.Module.Assembly.Dispose();
        pluginField.Module.Assembly.Dispose();
        
        var loaded = LoadMethod(Utility.PluginAssemblyName);
        Assert.True(HasFieldUsage(loaded, newFieldName));
    }

    private static bool HasMethodCall(MethodDefinition source, string targetFullName)
    {
        var instructions = source.Body.Instructions;
        foreach (var instruction in instructions)
        {
            if (instruction.OpCode != OpCodes.Call && instruction.OpCode != OpCodes.Callvirt) continue;
            if (instruction.Operand is not MethodReference methodReference) continue;
            if (methodReference.FullName == targetFullName) return true;
        }
        return false;
    }

    private static readonly OpCode[] FieldOpCodes = [OpCodes.Stfld, OpCodes.Ldfld, OpCodes.Stsfld, OpCodes.Ldsfld];
    
    private bool HasFieldUsage(MethodDefinition source, string targetFullName)
    {
        var instructions = source.Body.Instructions;
        foreach (var instruction in instructions)
        {
            _output.WriteLine(instruction.OpCode.ToString());
            if (!FieldOpCodes.Contains(instruction.OpCode)) continue;
            if (instruction.Operand is not FieldReference fieldReference) continue;
            if (fieldReference.FullName == targetFullName) return true;
        }
        return false;
    }
    
    private static FieldDefinition LoadField(
        string assemblyName,
        string typeName = Utility.TestClassName,
        string fieldName = Utility.TestFieldName
    )
    {
        var assembly = DllAssemblyFixture.Load(assemblyName);

        var type = assembly.Assembly.MainModule.Types
            .FirstOrDefault(x => x.Name == typeName);
        Assert.NotNull(type);

        var field = type.Fields
            .FirstOrDefault(x => x.Name == fieldName);
        Assert.NotNull(field);

        return field;
    }

    private static MethodDefinition LoadMethod(
        string assemblyName,
        string typeName = Utility.TestClassName,
        string methodName = Utility.TestMethodName
    )
    {
        var assembly = DllAssemblyFixture.Load(assemblyName);

        var type = assembly.Assembly.MainModule.Types
            .FirstOrDefault(x => x.Name == typeName);
        Assert.NotNull(type);

        var method = type.Methods
            .FirstOrDefault(x => x.Name == methodName);
        Assert.NotNull(method);

        return method;
    }
    
    private FieldDefinition CreateField(string assemblyName)
    {
        _output.WriteLine($"Creating field for assembly {assemblyName}");
        var assembly = new DllAssemblyFixture(assemblyName);
        var type = assembly.CreatePublicStaticClass();
        var field = type.CreatePublicStaticStringField();

        return field;
    }

    private MethodDefinition CreateMethod(string assemblyName)
    {
        _output.WriteLine($"Creating method for assembly {assemblyName}");
        var assembly = new DllAssemblyFixture(assemblyName);
        var type = assembly.CreatePublicStaticClass();
        var method = type.CreatePublicStaticVoidMethod();

        return method;
    }

    private static void DeleteAssembly(params string[] assemblyNames)
    {
        foreach (var assemblyName in assemblyNames)
        {
            var path = Path.Combine(Utility.OutputDirectory, $"{assemblyName}.dll");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}