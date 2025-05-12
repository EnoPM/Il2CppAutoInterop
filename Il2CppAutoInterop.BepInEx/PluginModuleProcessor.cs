using Il2CppAutoInterop.BepInEx.Processors;
using Il2CppAutoInterop.BepInEx.Utils;
using Il2CppAutoInterop.Cecil.Extensions;
using Il2CppAutoInterop.Core.BaseProcessors;
using Il2CppAutoInterop.Core.Contexts;
using Il2CppAutoInterop.Core.DependencyInjection;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx;

public sealed class PluginModuleProcessor : BaseModuleProcessor
{
    private readonly ResolvableTypes _types;
    
    public PluginModuleProcessor(AutoInteropModule module, DependencyCollection parent) : base(module)
    {
        Dependencies.RegisterParent(parent);
        _types = Dependencies.RegisterImmediately<ResolvableTypes>();
    }
    public override async Task<bool> ProcessAsync()
    {
        Console.WriteLine($"Processing module {nameof(PluginModuleProcessor)}");
        var types = GetMonoBehaviourTypes();

        return await ProcessAsync(types);
    }

    private async Task<bool> ProcessAsync(IEnumerable<TypeDefinition> types)
    {
        foreach (var type in types)
        {
            Console.WriteLine($"Processing MonoBehaviour component {type.FullName}");
            //TODO: Process MonoBehaviour components

            await ProcessSerializedFields(type);
        }

        return true;
    }

    private async Task ProcessSerializedFields(TypeDefinition type)
    {
        var serializedFields = GetSerializedFields(type);
        foreach (var field in serializedFields)
        {
            var scope = Dependencies.CreateScope();
            scope.RegisterImmediately(new AutoInteropField(field));
            var processor = scope.RegisterImmediately<SerializedFieldProcessor>();
            await processor.ProcessAsync();
        }
    }

    private IEnumerable<TypeDefinition> GetMonoBehaviourTypes()
    {
        return Module.Module.Types.Where(type => type.IsAssignableTo(_types.MonoBehaviour));
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
            if (field.HasCustomAttribute(_types.NonSerializedAttribute))
            {
                return false;
            }
            if (field.HasCustomAttribute(_types.SerializeFieldAttribute))
            {
                return true;
            }
        }
        return field.IsPublic;
    }
}