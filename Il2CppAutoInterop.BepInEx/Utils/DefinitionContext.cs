using Il2CppAutoInterop.Cecil.Extensions;
using Il2CppAutoInterop.Cecil.Interfaces;
using Mono.Cecil;

namespace Il2CppAutoInterop.BepInEx.Utils;

public sealed class DefinitionContext
{
    private readonly IAssemblyLoaderContext _loader;
    private readonly ModuleDefinition _mainModule;
    
    internal readonly TypeDefinition MonoBehaviour;
    internal readonly TypeDefinition SerializeFieldAttribute;
    internal readonly TypeDefinition NonSerializedAttribute;
    internal readonly TypeDefinition BepInExBasePlugin;
    
    internal DefinitionContext(IAssemblyLoaderContext loader, ModuleDefinition mainModule)
    {
        _loader = loader;
        _mainModule = mainModule;

        MonoBehaviour = Load("UnityEngine.MonoBehaviour");
        SerializeFieldAttribute = Load("UnityEngine.SerializeField");
        NonSerializedAttribute = Load("System.NonSerializedAttribute");
        BepInExBasePlugin = Load("BepInEx.Unity.IL2CPP.BasePlugin");
    }

    private TypeDefinition Load(string typeName)
    {
        var type = _mainModule.Resolve(typeName, _loader);
        if (type == null)
        {
            throw new Exception($"Unable to resolve type {typeName}");
        }

        return type;
    }
}