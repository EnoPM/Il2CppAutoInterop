using Il2CppAutoInterop.Cecil.Extensions;
using Il2CppAutoInterop.Core.Utils;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Utils;

internal static class UnityUtility
{
    internal static List<TypeDefinition> GetMonoBehaviourTypes(ModuleDefinition module, DefinitionContext definitions)
    {
        var types = module.GetAllTypes()
            .Where(definitions.IsMonoBehaviour)
            .ToList();

        var sorter = new TopologicalSorter<TypeDefinition>(types, ResolveMonoBehaviourDependencies);
        return sorter.Sort();
    }

    internal static List<FieldDefinition> ResolveSerializedFields(TypeDefinition type, DefinitionContext definitions)
    {
        return type.Fields
            .Where(definitions.IsSerializedField)
            .ToList();
    }

    private static List<TypeDefinition> ResolveMonoBehaviourDependencies(TypeDefinition type)
    {
        return [type.BaseType.Resolve()];
    }
    
    private static bool IsSerializedField(this DefinitionContext definitions, FieldDefinition field)
    {
        if (field.IsLiteral || field.IsStatic || field.IsFamilyOrAssembly || field.IsInitOnly) return false;
        if (field.HasCustomAttributes)
        {
            if (field.HasCustomAttribute(definitions.NonSerializedAttribute))
            {
                return false;
            }
            if (field.HasCustomAttribute(definitions.SerializeFieldAttribute))
            {
                return true;
            }
        }
        return field.IsPublic;
    }

    private static bool IsMonoBehaviour(this DefinitionContext definitions, TypeDefinition type)
    {
        return type.IsAssignableTo(definitions.MonoBehaviour);
    }
}