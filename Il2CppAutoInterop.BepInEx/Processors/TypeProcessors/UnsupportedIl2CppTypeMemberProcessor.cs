using Il2CppAutoInterop.Cecil.Extensions;
using Il2CppAutoInterop.Core;
using Il2CppAutoInterop.Logging;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Processors.TypeProcessors;

public sealed class UnsupportedIl2CppTypeMemberProcessor : IProcessor
{
    public readonly MonoBehaviourProcessor MonoBehaviourProcessor;
    public ModuleDefinition Module => MonoBehaviourProcessor.ModuleProcessor.Module;
    private readonly List<string> _supportedSystemTypeFullNames;

    public UnsupportedIl2CppTypeMemberProcessor(MonoBehaviourProcessor monoBehaviourProcessor)
    {
        MonoBehaviourProcessor = monoBehaviourProcessor;

        _supportedSystemTypeFullNames =
        [
            Module.TypeSystem.Void.FullName,
            Module.TypeSystem.String.FullName,
        ];
    }

    public void Process()
    {
        foreach (var method in MonoBehaviourProcessor.ComponentType.Methods)
        {
            if (IsSupportedInIl2Cpp(method)) continue;
            HideFromIl2Cpp(method);
        }
        foreach (var @event in MonoBehaviourProcessor.ComponentType.Events)
        {
            if (IsSupportedInIl2Cpp(@event)) continue;
            HideFromIl2Cpp(@event);
        }
        foreach (var property in MonoBehaviourProcessor.ComponentType.Properties)
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
        var attributeConstructor = MonoBehaviourProcessor.Definitions.HideFromIl2CppAttributeConstructor;
        var customAttribute = new CustomAttribute(Module.ImportReference(attributeConstructor));
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

            if (resolvedType.IsAssignableTo(MonoBehaviourProcessor.Definitions.Il2CppObjectBase))
            {
                return resolvedType.Module.Assembly != MonoBehaviourProcessor.ModuleProcessor.AssemblyProcessor.Assembly;
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