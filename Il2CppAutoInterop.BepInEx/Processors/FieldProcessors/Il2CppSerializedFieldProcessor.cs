using Il2CppAutoInterop.BepInEx.Contexts;
using Il2CppAutoInterop.BepInEx.Utils;
using Il2CppAutoInterop.Cecil.Extensions;
using Il2CppAutoInterop.Common;
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
    private FieldDefinition UsableField => Context.ProcessingField;


    public Il2CppSerializedFieldProcessor(UnitySerializedFieldContext context) : base(context)
    {
        _serializedFieldName = UsableField.Name;
        _interopFieldType = Il2CppInteropUtility.GetSerializedFieldInteropType(UsableField, Context.InteropTypes);
        _serializedFieldTypeReference = new Loadable<TypeReference>(CreateSerializedFieldTypeReference);
        _serializedField = new Loadable<FieldDefinition>(CreateSerializedField);
    }

    public override void Process()
    {
        RenameUsedField();
        Context.ProcessingType.Fields.Add(_serializedField.Value);
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
        UsableField.Name = $"__{nameof(Il2CppAutoInterop)}_UsableField_{_serializedFieldName}";
        // TODO: Rename field in all assemblies who reference and use this one
        // TODO: Check field accessibility and create custom attribute to allow private field to be serialized (to optimize post compilaiton)
    }
    
    private void AddDeserializationInstruction()
    {
        var il = Context.DeserializationMethod.Value.Body.GetILProcessor();
        var interopGetMethod = Context.ProcessingModule.ImportReference(_interopFieldType.Value.Methods.First(x => x.Name == "Get"));
        if (_interopFieldType.Value.HasGenericParameters)
        {
            interopGetMethod.DeclaringType = Context.ProcessingModule.ImportReference(
                _interopFieldType.Value.MakeGenericInstanceType(UsableField.FieldType)
            );
        }
        
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, _serializedField.Value);
        il.Emit(OpCodes.Callvirt, interopGetMethod);
        il.Emit(OpCodes.Stfld, UsableField);
    }

    private FieldDefinition CreateSerializedField()
    {
        if (UsableField.Name == _serializedFieldName)
        {
            throw new Exception($"The field {_serializedFieldName} has not been renamed yet.");
        }
        var field = new FieldDefinition(
            _serializedFieldName,
            FieldAttributes.Public,
            Context.ProcessingModule.ImportReference(_serializedFieldTypeReference.Value)
        );

        Context.ProcessingType.Fields.Add(field);

        return field;
    }

    private TypeReference CreateSerializedFieldTypeReference()
    {
        var interopType = _interopFieldType.Value;
        if (interopType.HasGenericParameters)
        {
            return interopType.CreateGenericInstanceType(UsableField.FieldType);
        }
        return interopType;
    }
}