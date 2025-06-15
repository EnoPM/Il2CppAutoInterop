using Il2CppAutoInterop.BepInEx.Contexts;
using Il2CppAutoInterop.BepInEx.Utils;
using Il2CppAutoInterop.Cecil.Extensions;
using Il2CppAutoInterop.Cecil.Utils;
using Il2CppAutoInterop.Common;
using Il2CppAutoInterop.Common.Logging;
using Il2CppAutoInterop.Core.Processors;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Il2CppAutoInterop.BepInEx.Processors.FieldProcessors;

public class Il2CppSerializedFieldProcessor : BaseFieldProcessor<UnitySerializedFieldContext>
{
    private readonly string _serializedFieldName;
    private readonly Loadable<TypeDefinition> _interopFieldType;
    private readonly Loadable<TypeReference> _serializedFieldTypeReference;
    private readonly Loadable<FieldDefinition> _serializedField;
    private readonly Loadable<bool> _isPluginMonoBehaviourFieldType;
    private FieldDefinition UsableField => Context.ProcessingField;


    public Il2CppSerializedFieldProcessor(UnitySerializedFieldContext context) : base(context)
    {
        _isPluginMonoBehaviourFieldType = new Loadable<bool>(CheckIfItsPluginMonoBehaviourFieldType);
        _serializedFieldName = UsableField.Name;
        _interopFieldType = Il2CppInteropUtility.GetSerializedFieldInteropType(UsableField, Context);
        _serializedFieldTypeReference = new Loadable<TypeReference>(CreateSerializedFieldTypeReference);
        _serializedField = new Loadable<FieldDefinition>(CreateSerializedField);
    }

    public override void Process()
    {
        RenameUsedField();
        _serializedField.Load();
        AddDeserializationInstruction();

        /*
         ISSUE: Serialized fields typed as injected component cannot be deserialized as their type
         TODO: Converting serialized fields typed with a Mono Component injected into il2cpp into a GameObject and then casting them upon deserialization
        */
    }

    public SerializedFieldGenerationData ToSerializedFieldData()
    {
        return new SerializedFieldGenerationData(UsableField, _serializedField.Value);
    }

    private void RenameUsedField()
    {
        var oldFullName = UsableField.FullName;
        var newFieldName = $"__{nameof(Il2CppAutoInterop)}_UsableField_{_serializedFieldName}";
        UsableField.Name = newFieldName;
        // TODO: Check field accessibility and create custom attribute to allow private field to be serialized (to optimize post compilaiton)

        var allDependents = Context.Loader.Dependencies
            .GetDependentAssemblies(Context.ProcessingAssembly.Name)
            .Where(x => Context.AssemblyFilePaths.Contains(x.MainModule.FileName));
        foreach (var assembly in allDependents)
        {
            var edited = false;
            foreach (var module in assembly.Modules)
            {
                foreach (var type in module.Types)
                {
                    foreach (var method in type.Methods)
                    {
                        while (method.HasFieldUsage(oldFullName))
                        {
                            edited = true;
                            var il = method.Body.GetILProcessor();
                            var instructionToReplace = method.Body.Instructions
                                .First(x => MethodsUtility.IsFieldRelatedInstruction(x)
                                && x.Operand is FieldReference reference
                                && reference.FullName == oldFullName);
                            var newInstruction = il.Create(
                                instructionToReplace.OpCode,
                                method.Module.ImportReference(UsableField));
                            il.Replace(instructionToReplace, newInstruction);
                            Logger.Instance.Warning($"Replacing field reference in {method.FullName}");
                        }
                    }
                }
            }
            if (!edited) continue;
            assembly.Write(assembly.MainModule.FileName);
        }
    }

    private void AddDeserializationInstruction()
    {
        var il = Context.DeserializationMethod.Value.Body.GetILProcessor();
        var interopGetMethod = Context.ProcessingModule.ImportReference(_interopFieldType.Value.Methods.First(x => x.Name == "Get"));
        if (_interopFieldType.Value.HasGenericParameters)
        {
            interopGetMethod.DeclaringType = Context.ProcessingModule.ImportReference(
                _interopFieldType.Value.MakeGenericInstanceType(_isPluginMonoBehaviourFieldType.Value ? Context.InteropTypes.GameObjectType.Value : UsableField.FieldType)
            );
        }

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, _serializedField.Value);
        il.Emit(OpCodes.Callvirt, interopGetMethod);
        if (_isPluginMonoBehaviourFieldType.Value)
        {
            var getComponentMethod = new GenericInstanceMethod(Context.ProcessingModule.ImportReference(Context.InteropTypes.GameObjectGetComponentMethod.Value));
            getComponentMethod.GenericArguments.Add(UsableField.FieldType);

            il.Emit(OpCodes.Callvirt, getComponentMethod);
        }
        il.Emit(OpCodes.Stfld, UsableField);
    }

    private FieldDefinition CreateSerializedField()
    {
        var field = new FieldDefinition(
            _serializedFieldName,
            FieldAttributes.Public,
            Context.ProcessingModule.ImportReference(_serializedFieldTypeReference.Value)
        );

        Context.ProcessingType.Fields.Add(field);
        Logger.Instance.Log($"Adding field <{field.Name}> to {Context.ProcessingType.Name}.");

        return field;
    }

    private TypeReference CreateSerializedFieldTypeReference()
    {
        var interopType = _interopFieldType.Value;
        if (interopType.HasGenericParameters)
        {
            var genericParameter = _isPluginMonoBehaviourFieldType.Value ? Context.InteropTypes.GameObjectType.Value : UsableField.FieldType;
            return interopType.CreateGenericInstanceType(genericParameter);
        }
        return interopType;
    }

    private bool CheckIfItsPluginMonoBehaviourFieldType() => Il2CppInteropUtility.IsPluginMonoBehaviourFieldType(UsableField, Context);
}