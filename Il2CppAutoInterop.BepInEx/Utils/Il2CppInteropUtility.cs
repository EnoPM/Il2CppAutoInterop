using Il2CppAutoInterop.BepInEx.Contexts;
using Il2CppAutoInterop.Cecil.Extensions;
using Il2CppAutoInterop.Cecil.Utils;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Utils;

internal static class Il2CppInteropUtility
{
    internal static LoadableType GetSerializedFieldInteropType(FieldDefinition field, InteropTypesContext definitions)
    {
        var type = field.FieldType.Resolve();
        if (type.FullName == field.Module.TypeSystem.String.FullName)
        {
            return definitions.Il2CppStringField;
        }
        if (type.IsAssignableTo(definitions.UnityEngineObject))
        {
            return definitions.Il2CppReferenceField;
        }
        return definitions.Il2CppValueField;
    }
}