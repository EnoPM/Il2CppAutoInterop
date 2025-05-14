using Il2CppAutoInterop.Cecil.Extensions;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Utils;

internal static class Il2CppInteropUtility
{
    internal static TypeDefinition GetSerializedFieldInteropType(FieldDefinition field, ResolvedDefinitions definitions)
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