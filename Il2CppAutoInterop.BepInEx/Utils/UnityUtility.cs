using Il2CppAutoInterop.BepInEx.Contexts;
using Il2CppAutoInterop.Cecil.Extensions;
using Il2CppAutoInterop.Core.Utils;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Utils;

internal static class UnityUtility
{
    internal static List<TypeDefinition> GetMonoBehaviourTypes(ModuleDefinition module, InteropTypesContext interopTypes)
    {
        var types = module.GetAllTypes()
            .Where(interopTypes.IsMonoBehaviour)
            .ToList();

        var sorter = new TopologicalSorter<TypeDefinition>(types, ResolveMonoBehaviourDependencies);
        return sorter.Sort();
    }

    internal static List<FieldDefinition> GetSerializedFields(TypeDefinition type, InteropTypesContext interopTypes)
    {
        return type.Fields
            .Where(interopTypes.IsSerializedField)
            .ToList();
    }

    private static List<TypeDefinition> ResolveMonoBehaviourDependencies(TypeDefinition type)
    {
        return [type.BaseType.Resolve()];
    }
    
    private static bool IsSerializedField(this InteropTypesContext interopTypes, FieldDefinition field)
    {
        if (field.IsLiteral || field.IsStatic || field.IsFamilyOrAssembly || field.IsInitOnly) return false;
        if (field.HasCustomAttributes)
        {
            if (field.HasCustomAttribute(interopTypes.NonSerializedAttribute))
            {
                return false;
            }
            if (field.HasCustomAttribute(interopTypes.SerializeFieldAttribute))
            {
                return true;
            }
        }
        return field.IsPublic;
    }

    private static bool IsMonoBehaviour(this InteropTypesContext interopTypes, TypeDefinition type)
    {
        return type.IsAssignableTo(interopTypes.MonoBehaviour);
    }

    public static string GetUnityEditorGeneratedDirectoryPath(string unityProjectDirectory)
    {
        return Path.Combine(Path.GetFullPath(unityProjectDirectory), "Assets", nameof(Il2CppAutoInterop), "Generated");
    }
}