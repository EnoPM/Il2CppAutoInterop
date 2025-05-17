using Il2CppAutoInterop.BepInEx.Contexts;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Il2CppAutoInterop.BepInEx.Processors.TypeProcessors.MonoBehaviourProcessors;

public class AbstractToVirtualProcessor : BaseMonoBehaviourProcessor
{

    public AbstractToVirtualProcessor(BepInExPluginMonoBehaviourContext context) : base(context)
    {
    }
    
    public override void Process()
    {
        foreach (var method in Context.ProcessingType.Methods)
        {
            if (!method.IsAbstract) continue;
            Process(method);
        }
    }

    private void Process(MethodDefinition method)
    {
        method.IsAbstract = false;
        method.IsVirtual = true;
        
        var il = method.Body.GetILProcessor();
        il.Emit(OpCodes.Newobj, Context.ProcessingModule.ImportReference(Context.InteropTypes.NotImplementedExceptionConstructor.Value));
        il.Emit(OpCodes.Throw);
    }
}