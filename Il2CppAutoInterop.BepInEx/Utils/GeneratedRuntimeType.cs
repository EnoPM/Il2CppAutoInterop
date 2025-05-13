using Il2CppAutoInterop.BepInEx.Extensions;
using Il2CppAutoInterop.BepInEx.Processors;
using Il2CppAutoInterop.Cecil.Utils;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Utils;

public sealed class GeneratedRuntimeType
{
    public readonly string RuntimeNamespace;
    public readonly TypeDefinition Definition;
    public readonly MethodDefinition PluginEntryPoint;

    public GeneratedRuntimeType(ModuleProcessor module)
    {
        RuntimeNamespace = string.Join('.', nameof(Il2CppAutoInterop), module.Module.Name, "Runtime");

        Definition = new TypeDefinition(
            RuntimeNamespace,
            "AutoInterop",
            TypeAttributesUtility.Internal | TypeAttributesUtility.Static | TypeAttributes.Class,
            module.Module.TypeSystem.Object
        );

        PluginEntryPoint = module.Module.GetBepInExPluginEntryPointMethod(module.AssemblyProcessor.Definitions) ??
                           throw new InvalidOperationException(
                               $"{module.Module.Name} doesn't contains an BepInEx Il2Cpp plugin entry point");
    }
}