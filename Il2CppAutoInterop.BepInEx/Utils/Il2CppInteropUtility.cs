using Il2CppAutoInterop.BepInEx.Contexts;
using Il2CppAutoInterop.Cecil.Extensions;
using Il2CppAutoInterop.Cecil.Utils;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Utils;

internal static class Il2CppInteropUtility
{
    internal static LoadableType GetSerializedFieldInteropType(FieldDefinition field, UnitySerializedFieldContext context)
    {
        var type = field.Module.Resolve(field.FieldType.FullName, context.Loader);
        //var type = field.FieldType.Resolve();
        if (type == null)
        {
            throw new Exception($"Unable to resolve field type {field.FieldType.FullName}");
        }
        if (type.FullName == field.Module.TypeSystem.String.FullName)
        {
            return context.InteropTypes.Il2CppStringField;
        }
        if (type.IsAssignableTo(context.InteropTypes.UnityEngineObject))
        {
            return context.InteropTypes.Il2CppReferenceField;
        }
        return context.InteropTypes.Il2CppValueField;
    }
    
    public static bool IsPluginMonoBehaviourFieldType(FieldDefinition field, BepInExPluginSerializationContext context)
    {
        var fieldType = field.FieldType.Module.Resolve(field.FieldType.FullName, context.Loader);
        if (fieldType == null)
        {
            throw new Exception($"Unable to resolve field type {field.FieldType.FullName}");
        }
        if (!fieldType.IsAssignableTo(context.InteropTypes.MonoBehaviour))
        {
            return false;
        }
        return context.AssemblyFilePaths.Any(x => x == fieldType.Module.FileName);
    }
}