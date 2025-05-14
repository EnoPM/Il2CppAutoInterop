using Il2CppAutoInterop.BepInEx.Extensions;
using Il2CppAutoInterop.BepInEx.Processors;
using Il2CppAutoInterop.Cecil.Utils;
using Il2CppAutoInterop.Core.Utils;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Il2CppAutoInterop.BepInEx.Utils;

public sealed class GeneratedRuntimeManager
{
    public readonly string BaseRuntimeNamespace;
    public readonly MethodDefinition PluginEntryPoint;
    public readonly ModuleProcessor ModuleProcessor;

    public readonly OptionalDefinition<TypeDefinition> ComponentRegistererType;
    public readonly OptionalDefinition<MethodDefinition> ComponentRegistererMethod;

    public GeneratedRuntimeManager(ModuleProcessor module)
    {
        ModuleProcessor = module;
        BaseRuntimeNamespace = Namespace(nameof(Il2CppAutoInterop), ModuleProcessor.Module.Name, "Runtime");

        ComponentRegistererType = new OptionalDefinition<TypeDefinition>(CreateComponentRegistererType);
        ComponentRegistererMethod = new OptionalDefinition<MethodDefinition>(CreateComponentRegistererMethod);

        PluginEntryPoint = module.Module.GetBepInExPluginEntryPointMethod(module.AssemblyProcessor.Definitions) ??
                           throw new InvalidOperationException(
                               $"{module.Module.Name} doesn't contains an BepInEx Il2Cpp plugin entry point");
    }

    private TypeDefinition CreateComponentRegistererType()
    {
        var type = new TypeDefinition(
            BaseRuntimeNamespace,
            "AutoInterop",
            TypeAttributesUtility.Internal | TypeAttributesUtility.Static | TypeAttributes.Class,
            ModuleProcessor.Module.TypeSystem.Object
        );

        ModuleProcessor.Module.Types.Add(type);

        return type;
    }
    
    private MethodDefinition CreateComponentRegistererMethod()
    {
        var method = new MethodDefinition(
            "RegisterAllComponentsInIl2Cpp",
            MethodAttributes.Static | MethodAttributes.Assembly,
            ModuleProcessor.Module.TypeSystem.Void
        );
        
        var registerTypeOptionsType = ModuleProcessor.Module.ImportReference(ModuleProcessor.Definitions.ClassInjectorRegisterOptionsConstructor.DeclaringType);
        method.Body.Variables.Add(new VariableDefinition(registerTypeOptionsType));
        
        var il = method.Body.GetILProcessor();
        il.Emit(OpCodes.Ret);
        
        ComponentRegistererType.Definition.Methods.Add(method);

        return method;
    }
    
    private static string Namespace(params string[] fragments) => string.Join('.', fragments);
}