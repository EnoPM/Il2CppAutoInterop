using Il2CppAutoInterop.Core.Utils;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Processors.MonoBehaviourComponents;

public sealed class MonoBehaviourComponentProcessor
{
    public readonly ModuleProcessor ModuleProcessor;
    public readonly TypeDefinition ComponentType;
    
    public MonoBehaviourComponentProcessor(ModuleProcessor moduleProcessor, TypeDefinition componentType)
    {
        ModuleProcessor = moduleProcessor;
        ComponentType = componentType;
    }

    public void Process()
    {
        using (new TimedExecution($"Running {nameof(MonoBehaviourComponentProcessor)} processor for type {ComponentType.Name} : {ComponentType.BaseType.Name}", ConsoleColor.Magenta))
        {
            //TODO: Handle all MonoBehaviour processors (SerializedFieldsProcessor, UnsupportedMembersProcessor, AbstractMembersProcessor)
        }
    }
}