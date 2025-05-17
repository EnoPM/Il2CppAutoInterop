using Il2CppAutoInterop.BepInEx.Contexts;
using Il2CppAutoInterop.Cecil.Extensions;
using Il2CppAutoInterop.Common.Logging;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Processors.TypeProcessors.MonoBehaviourProcessors;

public sealed class UnsupportedIl2CppMemberProcessor : BaseMonoBehaviourProcessor
{

    private readonly HashSet<string> _supportedSystemTypeFullNames;

    public UnsupportedIl2CppMemberProcessor(BepInExPluginMonoBehaviourContext context) : base(context)
    {
        _supportedSystemTypeFullNames =
        [
            Context.ProcessingModule.TypeSystem.Void.FullName,
            Context.ProcessingModule.TypeSystem.String.FullName
        ];
    }


    public override void Process()
    {
        foreach (var method in Context.ProcessingType.Methods)
        {
            if (IsSupportedInIl2Cpp(method)) continue;
            HideFromIl2Cpp(method);
        }
        foreach (var @event in Context.ProcessingType.Events)
        {
            if (IsSupportedInIl2Cpp(@event)) continue;
            HideFromIl2Cpp(@event);
        }
        foreach (var property in Context.ProcessingType.Properties)
        {
            if (IsSupportedInIl2Cpp(property)) continue;
            HideFromIl2Cpp(property);
        }
    }

    private bool IsSupportedInIl2Cpp(PropertyDefinition property)
    {
        return IsTypeSupportedInIl2Cpp(property.PropertyType);
    }

    private bool IsSupportedInIl2Cpp(EventDefinition @event)
    {
        return IsTypeSupportedInIl2Cpp(@event.EventType);
    }

    private bool IsSupportedInIl2Cpp(MethodDefinition method)
    {
        return IsTypeSupportedInIl2Cpp(method.ReturnType) && method.Parameters.All(IsSupportedInIl2Cpp);
    }

    private bool IsSupportedInIl2Cpp(ParameterDefinition parameter)
    {
        return IsTypeSupportedInIl2Cpp(parameter.ParameterType);
    }

    private void HideFromIl2Cpp(IMemberDefinition member)
    {
        var attributeConstructor = Context.InteropTypes.HideFromIl2CppAttributeConstructor;
        var customAttribute = new CustomAttribute(Context.ProcessingModule.ImportReference(attributeConstructor.Value));
        member.CustomAttributes.Add(customAttribute);
    }

    private bool IsTypeSupportedInIl2Cpp(TypeReference type)
    {
        if (_supportedSystemTypeFullNames.Contains(type.FullName) || type.IsGenericParameter)
        {
            return true;
        }

        if (type.IsByReference)
        {
            return IsTypeSupportedInIl2Cpp(((ByReferenceType)type).ElementType);
        }
        try
        {
            var resolvedType = type.Resolve();

            if (resolvedType.IsEnum)
            {
                return false;
            }

            if (resolvedType.IsValueType)
            {
                return true;
            }

            if (resolvedType.IsAssignableTo(Context.InteropTypes.Il2CppObjectBase))
            {
                return resolvedType.Module.Assembly != Context.ProcessingAssembly;
            }
        }
        catch (Exception ex)
        {
            Logger.Instance.Warning($"Unresolved type: '{type.FullName}'. {ex.Message}");
            return false;
        }

        return false;
    }
}