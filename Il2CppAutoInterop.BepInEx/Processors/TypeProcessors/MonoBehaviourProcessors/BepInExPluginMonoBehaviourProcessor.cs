using Il2CppAutoInterop.BepInEx.Contexts;
using Il2CppAutoInterop.Core.Processors;

namespace Il2CppAutoInterop.BepInEx.Processors.TypeProcessors.MonoBehaviourProcessors;

public sealed class BepInExPluginMonoBehaviourProcessor : BaseTypeProcessor<BepInExPluginMonoBehaviourContext>
{
    public BepInExPluginMonoBehaviourProcessor(BepInExPluginMonoBehaviourContext context) : base(context)
    {
    }
    
    public override void Process()
    {
        ProcessUnsupportedIl2CppMembers();
        ProcessDeserialization();
        ProcessAbstractToVirtualConversion();
        ProcessIntPtrConstructor();
        ProcessIl2CppComponentsRegistration();
    }

    private void ProcessIl2CppComponentsRegistration()
    {
        var processor = new Il2CppRegistrationProcessor(Context);
        processor.Process();
    }

    private void ProcessIntPtrConstructor()
    {
        var processor = new IntPtrConstructorProcessor(Context);
        processor.Process();
    }

    private void ProcessAbstractToVirtualConversion()
    {
        var processor = new AbstractToVirtualProcessor(Context);
        processor.Process();
    }

    private void ProcessDeserialization()
    {
        var processor = new SerializationProcessor(Context);
        processor.Process();
    }
    
    private void ProcessUnsupportedIl2CppMembers()
    {
        var processor = new UnsupportedIl2CppMemberProcessor(Context);
        processor.Process();
    }
}