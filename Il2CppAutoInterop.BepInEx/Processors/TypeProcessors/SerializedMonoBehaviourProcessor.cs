using Il2CppAutoInterop.BepInEx.CSharpGenerators;
using Il2CppAutoInterop.BepInEx.Processors.FieldProcessors;
using Il2CppAutoInterop.BepInEx.Utils;
using Il2CppAutoInterop.Cecil.Extensions;
using Il2CppAutoInterop.Core;
using Il2CppAutoInterop.Core.Utils;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Il2CppAutoInterop.BepInEx.Processors.TypeProcessors;

public sealed class SerializedMonoBehaviourProcessor : IProcessor
{
    public readonly MonoBehaviourProcessor MonoBehaviourProcessor;
    public readonly List<SerializedFieldProcessor> SerializedFields;

    public ResolvedDefinitions Definitions => MonoBehaviourProcessor.Definitions;
    public GeneratedRuntimeManager Runtime => MonoBehaviourProcessor.ModuleProcessor.Runtime;
    public ModuleDefinition Module => MonoBehaviourProcessor.ModuleProcessor.Module;
    public TypeDefinition ComponentType => MonoBehaviourProcessor.ComponentType;

    public readonly OptionalDefinition<MethodDefinition> DeserializationMethod;

    public SerializedMonoBehaviourProcessor(MonoBehaviourProcessor monoBehaviourProcessor)
    {
        MonoBehaviourProcessor = monoBehaviourProcessor;
        SerializedFields = UnityUtility
            .GetSerializedFields(MonoBehaviourProcessor.ComponentType, MonoBehaviourProcessor.Definitions)
            .Select(x => new SerializedFieldProcessor(this, x))
            .ToList();
        DeserializationMethod = new OptionalDefinition<MethodDefinition>(CreateDeserializationMethod);
    }


    public void Process()
    {
        if (SerializedFields.Count == 0)
        {
            return;
        }

        foreach (var processor in SerializedFields)
        {
            processor.Process();
        }

        CreateSerialisationInterfaceMethods();
        AddDeserializeMethodCall();
        CreateUnityProjectRelatedFile();
    }

    private void CreateUnityProjectRelatedFile()
    {
        var unityProjectDirectory = MonoBehaviourProcessor
            .ModuleProcessor
            .AssemblyProcessor
            .PluginProcessor
            .UnityProjectDirectory;
        if (unityProjectDirectory == null)
        {
            return;
        }
        
        var generatedDirectory = Path.Combine(unityProjectDirectory, "Assets", nameof(Il2CppAutoInterop), "Generated");
        if (!Directory.Exists(generatedDirectory))
        {
            Directory.CreateDirectory(generatedDirectory);
        }

        var generator = new UnityProjectMonoBehaviour(ComponentType, this);
        var fileContent = generator.GenerateFileContent();
        
        var fileName = $"{ComponentType.Namespace.Replace("/", ".")}.cs";
        var path = ComponentType.Namespace.Split(".");
        var fileDirectoryPath = Path.Combine(generatedDirectory, Path.Combine(path));
        if (!Directory.Exists(fileDirectoryPath))
        {
            Directory.CreateDirectory(fileDirectoryPath);
        }
        var filePath = Path.Combine(fileDirectoryPath, fileName);
        File.WriteAllText(filePath, fileContent);
    }

    private void CreateSerialisationInterfaceMethods()
    {
        if (!MonoBehaviourProcessor.UseUnitySerializationInterface) return;
        CreateBeforeSerializationMethod();
        CreateAfterDeserializationMethod();
    }

    private void CreateBeforeSerializationMethod()
    {
        var method = new MethodDefinition(
            "OnBeforeSerialize",
            MethodAttributes.Public,
            Module.TypeSystem.Void);
        
        var il = method.Body.GetILProcessor();
        il.Emit(OpCodes.Ret);
        
        ComponentType.Methods.Add(method);
    }

    private void CreateAfterDeserializationMethod()
    {
        DeserializationMethod.Create();
    }

    private void AddDeserializeMethodCall()
    {
        if (MonoBehaviourProcessor.UseUnitySerializationInterface) return;
        var awakeMethod = FindOrCreateAwakeMethod(out var parentAwakeMethod);
        var il = awakeMethod.Body.GetILProcessor();
        il.Prepend([
            il.Create(OpCodes.Ldarg_0),
            il.Create(OpCodes.Call, Module.ImportReference(DeserializationMethod.Definition))
        ]);
        if (parentAwakeMethod == null) return;
        var hasParentAwakeMethodCall = awakeMethod.Body.Instructions
            .Any(x => x.OpCode == OpCodes.Call && x.Operand is MethodReference xMethod &&
                      xMethod.FullName == parentAwakeMethod.FullName);
        if (hasParentAwakeMethodCall) return;
        il.Prepend([
            il.Create(OpCodes.Ldarg_0),
            il.Create(OpCodes.Call, Module.ImportReference(parentAwakeMethod))
        ]);
    }

    private MethodDefinition FindOrCreateAwakeMethod(out MethodDefinition? parentAwakeMethodResult)
    {
        if (!ComponentType.TryFindNearestMethod(NearestAwakeMethodFinder, out var nearestAwakeMethod))
        {
            // No awake method found in parent or in current class
            var newAwakeMethod = CreateEmptyAwakeMethod(MethodAttributes.Private);
            ComponentType.Methods.Add(newAwakeMethod);
            parentAwakeMethodResult = null;
            return newAwakeMethod;
        }

        if (nearestAwakeMethod.DeclaringType.FullName == ComponentType.FullName)
        {
            // Awake method found in current class
            var parentAwakeMethod = EnsureParentAwakeMethodAccess();
            if (parentAwakeMethod != null)
            {
                // Parent class has a generated Awake method. So we need to make current Awake an override and ensure parent Awake method is call in child Awake method override.
                nearestAwakeMethod.IsPrivate = false;
                nearestAwakeMethod.IsPublic = parentAwakeMethod.IsPublic;
                nearestAwakeMethod.IsFamily = parentAwakeMethod.IsFamily;
                nearestAwakeMethod.IsVirtual = true;
                nearestAwakeMethod.IsHideBySig = true;
            }

            parentAwakeMethodResult = parentAwakeMethod;
            return nearestAwakeMethod;
        }

        // Awake method found in parent class so we need to fix the access of it
        if (nearestAwakeMethod.IsPrivate)
        {
            nearestAwakeMethod.IsPrivate = false;
            nearestAwakeMethod.IsFamily = true;
        }

        if (!nearestAwakeMethod.IsVirtual)
        {
            nearestAwakeMethod.IsVirtual = true;
        }

        var attributes = MethodAttributes.Virtual | MethodAttributes.HideBySig;
        if (nearestAwakeMethod.IsFamily)
        {
            attributes |= MethodAttributes.Family;
        }

        var awakeMethod = CreateEmptyAwakeMethod(attributes);
        ComponentType.Methods.Add(awakeMethod);

        parentAwakeMethodResult = nearestAwakeMethod;

        return awakeMethod;
    }

    private MethodDefinition? EnsureParentAwakeMethodAccess()
    {
        if (!ComponentType.TryFindNearestMethod(NearestParentAwakeMethodFinder, out var parentAwakeMethod))
        {
            return null;
        }

        if (parentAwakeMethod.IsPrivate)
        {
            parentAwakeMethod.IsPrivate = false;
            parentAwakeMethod.IsFamily = true;
        }

        if (!parentAwakeMethod.IsVirtual)
        {
            parentAwakeMethod.IsVirtual = true;
        }

        return parentAwakeMethod;
    }

    private static bool NearestAwakeMethodFinder(MethodDefinition method)
    {
        return method.Name == "Awake" && !method.HasParameters;
    }

    private bool NearestParentAwakeMethodFinder(MethodDefinition method)
    {
        return method.DeclaringType.FullName != ComponentType.FullName && NearestAwakeMethodFinder(method);
    }

    private MethodDefinition CreateEmptyAwakeMethod(MethodAttributes attributes)
    {
        var awakeMethod = new MethodDefinition("Awake", attributes, Module.TypeSystem.Void);

        var il = awakeMethod.Body.GetILProcessor();
        il.Emit(OpCodes.Ret);

        return awakeMethod;
    }

    private MethodDefinition CreateDeserializationMethod()
    {
        var methodName = $"__{nameof(Il2CppAutoInterop)}_{ComponentType.Name}_AfterDeserializeMethod";
        if (MonoBehaviourProcessor.UseUnitySerializationInterface)
        {
            methodName = "OnAfterDeserialize";
        }
        
        var method = new MethodDefinition(
            methodName,
            MethodAttributes.Private,
            Module.TypeSystem.Void
        );

        ComponentType.Methods.Add(method);

        var il = method.Body.GetILProcessor();

        foreach (var processor in SerializedFields)
        {
            processor.AddDeserializationIlCode(il);
        }

        il.Emit(OpCodes.Ret);

        return method;
    }
}