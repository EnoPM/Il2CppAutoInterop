using Il2CppAutoInterop.BepInEx.Contexts;
using Il2CppAutoInterop.BepInEx.Processors.TypeProcessors.MonoBehaviourProcessors;
using Il2CppAutoInterop.BepInEx.Utils;
using Il2CppAutoInterop.Core.Processors;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Processors.ModuleProcessors;

public sealed class BepInExPluginModuleProcessor : BaseModuleProcessor<BepInExPluginModuleContext>
{

    public BepInExPluginModuleProcessor(BepInExPluginModuleContext context) : base(context)
    {
    }

    public override void Process()
    {

        ProcessMonoBehaviourTypes();
    }

    private void ProcessMonoBehaviourTypes()
    {
        var types = UnityUtility
            .GetMonoBehaviourTypes(Context.ProcessingModule, Context.InteropTypes);

        foreach (var type in types)
        {
            ProcessMonoBehaviourType(type);
        }
    }

    private void ProcessMonoBehaviourType(TypeDefinition type)
    {
        var context = new BepInExPluginMonoBehaviourContext(Context, type);
        var processor = new BepInExPluginMonoBehaviourProcessor(context);
        processor.Process();
    }
}