using Il2CppAutoInterop.BepInEx.Utils;
using Il2CppAutoInterop.Cecil.Extensions;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Extensions;

public static class ModuleDefinitionExtensions
{
    public static MethodDefinition? GetBepInExPluginEntryPointMethod(this ModuleDefinition module,
        DefinitionContext types)
    {
        var type = module.GetAllTypes()
            .FirstOrDefault(x => x.IsAssignableTo(types.BepInExBasePlugin));
        
        var method = type?.Methods
            .FirstOrDefault(x => x is { IsVirtual: true, IsReuseSlot: true, Name: "Load" });

        return method;
    }
}