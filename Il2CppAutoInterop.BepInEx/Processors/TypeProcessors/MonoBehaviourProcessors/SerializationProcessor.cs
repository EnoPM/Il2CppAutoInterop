using Il2CppAutoInterop.BepInEx.Contexts;
using Il2CppAutoInterop.BepInEx.Processors.FieldProcessors;
using Il2CppAutoInterop.BepInEx.Utils;
using Il2CppAutoInterop.Cecil.Extensions;
using Il2CppAutoInterop.Common;
using Il2CppAutoInterop.Common.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Il2CppAutoInterop.BepInEx.Processors.TypeProcessors.MonoBehaviourProcessors;

public sealed class SerializationProcessor : BaseMonoBehaviourProcessor
{
    private readonly Loadable<BepInExPluginSerializationContext> _context;

    public SerializationProcessor(BepInExPluginMonoBehaviourContext context) : base(context)
    {
        _context = new Loadable<BepInExPluginSerializationContext>(CreateSerializationContext);
    }

    public override void Process()
    {
        var serializedFields = UnityUtility.GetSerializedFields(Context.ProcessingType, Context.InteropTypes);
        if (serializedFields.Count == 0)
        {
            CreateUnityProjectRelatedFile([]);
            return;
        }

        ProcessSerializedFieldInterops(serializedFields);
        
        // Used to implement 'ISerializationCallbackReceiver' interface in 'Il2CppRegistrationProcessor'
        Context.InteropSummary.SerializedMonoBehaviourFullNames.Add(Context.ProcessingType.FullName);
    }

    private void ProcessSerializedFieldInterops(List<FieldDefinition> serializedFields)
    {
        var serializedFieldData = new List<SerializedFieldGenerationData>();
        foreach (var field in serializedFields)
        {
            var context = new UnitySerializedFieldContext(_context.Value, field);
            var processor = new Il2CppSerializedFieldProcessor(context);
            processor.Process();
            serializedFieldData.Add(processor.ToSerializedFieldData());
        }

        if (!_context.HasValue || !_context.Value.DeserializationMethod.HasValue) return;
        var il = _context.Value.DeserializationMethod.Value.Body.GetILProcessor();
        il.Emit(OpCodes.Ret);

        CreateSerializationInterfaceMethods();
        AddDeserializeMethodCall();
        CreateUnityProjectRelatedFile(serializedFieldData);
    }
    
    private void CreateUnityProjectRelatedFile(List<SerializedFieldGenerationData> serializedFields)
    {
        if (Context.UnityProjectDirectoryPath == null)
        {
            return;
        }

        var generatedDirectory = UnityUtility.GetUnityEditorGeneratedDirectoryPath(Context.UnityProjectDirectoryPath);
        if (!Directory.Exists(generatedDirectory))
        {
            Directory.CreateDirectory(generatedDirectory);
        }

        var generator = new MonoBehaviourCSharpGenerator(_context.Value, serializedFields);
        var fileContent = generator.GenerateFileContent();
        
        var fileName = $"{Context.ProcessingType.Name.Replace("/", ".")}.cs";
        var paths = Context.ProcessingType.Namespace.Split(".");
        var fileDirectoryPath = Path.Combine(generatedDirectory, Path.Combine(paths));
        if (!Directory.Exists(fileDirectoryPath))
        {
            Directory.CreateDirectory(fileDirectoryPath);
        }
        var filePath = Path.Combine(fileDirectoryPath, fileName);
        Context.InteropSummary.UnityProjectGeneratedFilePaths.Add(filePath);
        Logger.Instance.Info($"Creating Unity project related file for {filePath}");
        File.WriteAllText(filePath, fileContent);
    }

    private void AddDeserializeMethodCall()
    {
        if (Context.UseUnitySerializationInterface) return;
        var awakeMethod = FindOrCreateAwakeMethod(out var parentAwakeMethod);
        var il = awakeMethod.Body.GetILProcessor();
        il.Prepend([
            il.Create(OpCodes.Ldarg_0),
            il.Create(OpCodes.Call, Context.ProcessingModule.ImportReference(_context.Value.DeserializationMethod.Value))
        ]);
        if (parentAwakeMethod == null) return;
        var hasParentAwakeMethodCall = awakeMethod.Body.Instructions
            .Any(x => x.OpCode == OpCodes.Call && x.Operand is MethodReference xMethod &&
                      xMethod.FullName == parentAwakeMethod.FullName);
        if (hasParentAwakeMethodCall) return;
        il.Prepend([
            il.Create(OpCodes.Ldarg_0),
            il.Create(OpCodes.Call, Context.ProcessingModule.ImportReference(parentAwakeMethod))
        ]);
    }

    private MethodDefinition FindOrCreateAwakeMethod(out MethodDefinition? parentAwakeMethodResult)
    {
        if (!Context.ProcessingType.TryFindNearestMethod(NearestAwakeMethodFinder, out var nearestAwakeMethod))
        {
            // No awake method found in parent or in current class
            var newAwakeMethod = CreateEmptyAwakeMethod(MethodAttributes.Private);
            Context.ProcessingType.Methods.Add(newAwakeMethod);
            parentAwakeMethodResult = null;
            return newAwakeMethod;
        }

        if (nearestAwakeMethod.DeclaringType.FullName == Context.ProcessingType.FullName)
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
        Context.ProcessingType.Methods.Add(awakeMethod);

        parentAwakeMethodResult = nearestAwakeMethod;

        return awakeMethod;
    }

    private MethodDefinition? EnsureParentAwakeMethodAccess()
    {
        if (!Context.ProcessingType.TryFindNearestMethod(NearestParentAwakeMethodFinder, out var parentAwakeMethod))
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
        return method.DeclaringType.FullName != Context.ProcessingType.FullName && NearestAwakeMethodFinder(method);
    }

    private MethodDefinition CreateEmptyAwakeMethod(MethodAttributes attributes)
    {
        var awakeMethod = new MethodDefinition("Awake", attributes, Context.ProcessingModule.TypeSystem.Void);

        var il = awakeMethod.Body.GetILProcessor();
        il.Emit(OpCodes.Ret);

        return awakeMethod;
    }

    private void CreateSerializationInterfaceMethods()
    {
        if (!Context.UseUnitySerializationInterface) return;
        CreateBeforeSerializationMethod();
        CreateAfterDeserializationMethod();
    }

    private void CreateBeforeSerializationMethod()
    {
        var method = new MethodDefinition(
            "OnBeforeSerialize",
            MethodAttributes.Public,
            Context.ProcessingModule.TypeSystem.Void);

        var il = method.Body.GetILProcessor();
        il.Emit(OpCodes.Ret);

        Context.ProcessingType.Methods.Add(method);
    }

    private void CreateAfterDeserializationMethod()
    {
        if (_context.HasValue && _context.Value.DeserializationMethod.HasValue) return;
        _context.Value.DeserializationMethod.Load();
    }

    private MethodDefinition CreateDeserializationMethod()
    {
        var methodName = $"__{nameof(Il2CppAutoInterop)}_{Context.ProcessingType.Name}_AfterDeserializeMethod";
        if (Context.UseUnitySerializationInterface)
        {
            methodName = "OnAfterDeserialize";
        }

        var method = new MethodDefinition(
            methodName,
            MethodAttributes.Private,
            Context.ProcessingModule.TypeSystem.Void
        );

        Context.ProcessingType.Methods.Add(method);

        return method;
    }

    private BepInExPluginSerializationContext CreateSerializationContext()
    {
        return new BepInExPluginSerializationContext(
            Context,
            new Loadable<MethodDefinition>(CreateDeserializationMethod)
        );
    }
}