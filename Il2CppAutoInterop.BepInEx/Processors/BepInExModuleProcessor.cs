using Il2CppAutoInterop.BepInEx.Utils;
using Il2CppAutoInterop.Cecil.Extensions;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Processors;

public sealed class BepInExModuleProcessor
{
    public readonly BepInExAssemblyProcessor AssemblyProcessor;
    public readonly ModuleDefinition Module;
    public readonly GeneratedRuntimeType Runtime;

    public BepInExModuleProcessor(BepInExAssemblyProcessor assemblyProcessor, ModuleDefinition module)
    {
        AssemblyProcessor = assemblyProcessor;
        Module = module;
        Runtime = new GeneratedRuntimeType(this);
    }

    public void Process()
    {
        Console.WriteLine($"Running {nameof(BepInExModuleProcessor)} processor for module {Module.Name}");
    }
    
    private IEnumerable<TypeDefinition> GetMonoBehaviourTypes()
    {
        return Module.GetAllTypes()
            .Where(type => type.IsAssignableTo(AssemblyProcessor.Definitions.MonoBehaviour));
    }
    
    private IEnumerable<FieldDefinition> GetSerializedFields(TypeDefinition type)
    {
        return type.Fields.Where(IsUnitySerializedField);
    }
    
    private bool IsUnitySerializedField(FieldDefinition field)
    {
        if (field.IsLiteral || field.IsStatic || field.IsFamilyOrAssembly || field.IsInitOnly) return false;
        if (field.HasCustomAttributes)
        {
            if (field.HasCustomAttribute(AssemblyProcessor.Definitions.NonSerializedAttribute))
            {
                return false;
            }
            if (field.HasCustomAttribute(AssemblyProcessor.Definitions.SerializeFieldAttribute))
            {
                return true;
            }
        }
        return field.IsPublic;
    }
}