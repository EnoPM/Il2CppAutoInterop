using Il2CppAutoInterop.Core;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Processors.TypeProcessors;

public sealed class MonoBehaviourProcessor : IProcessor
{
    public readonly ModuleProcessor ModuleProcessor;
    public readonly TypeDefinition ComponentType;

    public ResolvedDefinitions Definitions => ModuleProcessor.Definitions;

    public MonoBehaviourProcessor(ModuleProcessor moduleProcessor, TypeDefinition componentType)
    {
        ModuleProcessor = moduleProcessor;
        ComponentType = componentType;
    }

    public void Process()
    {
        var serialization = ProcessSerializedMonoBehaviour();
        ProcessUnsupportedMembersInIl2Cpp();
        ProcessAbstractMonoBehaviour();
        ProcessIntPtrConstructor();
        ProcessIl2CppRegisterer(serialization);
    }

    private SerializedMonoBehaviourProcessor ProcessSerializedMonoBehaviour()
    {
        var processor = new SerializedMonoBehaviourProcessor(this);
        processor.Process();

        return processor;
    }

    private void ProcessUnsupportedMembersInIl2Cpp()
    {
        var processor = new UnsupportedIl2CppTypeMemberProcessor(this);
        processor.Process();
    }

    private void ProcessAbstractMonoBehaviour()
    {
        var processor = new AbstractMonoBehaviourProcessor(this);
        processor.Process();
    }

    private void ProcessIntPtrConstructor()
    {
        var processor = new IntPtrConstructorProcessor(this);
        processor.Process();
    }

    private void ProcessIl2CppRegisterer(SerializedMonoBehaviourProcessor serialization)
    {
        var processor = new Il2CppRegistererProcessor(this, serialization);
        processor.Process();
    }
}