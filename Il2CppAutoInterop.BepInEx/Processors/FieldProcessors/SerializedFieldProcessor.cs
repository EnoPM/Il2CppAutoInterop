using Il2CppAutoInterop.BepInEx.Processors.TypeProcessors;
using Il2CppAutoInterop.BepInEx.Utils;
using Il2CppAutoInterop.Cecil.Extensions;
using Il2CppAutoInterop.Core;
using Il2CppAutoInterop.Core.Utils;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Il2CppAutoInterop.BepInEx.Processors.FieldProcessors;

public sealed class SerializedFieldProcessor : IProcessor
{
    public readonly SerializedMonoBehaviourProcessor SerializedMonoBehaviourProcessor;
    public readonly FieldDefinition UsableField;
    public readonly string SerializedFieldName;
    public readonly FieldDefinition SerializedField;
    public readonly TypeDefinition SerializedFieldInteropType;

    public ResolvedDefinitions Definitions => SerializedMonoBehaviourProcessor.Definitions;
    public GeneratedRuntimeManager Runtime => SerializedMonoBehaviourProcessor.Runtime;
    public ModuleDefinition Module => SerializedMonoBehaviourProcessor.Module;
    public TypeDefinition ComponentType => SerializedMonoBehaviourProcessor.ComponentType;

    public SerializedFieldProcessor(SerializedMonoBehaviourProcessor serializedMonoBehaviourProcessor, FieldDefinition usableField)
    {
        SerializedMonoBehaviourProcessor = serializedMonoBehaviourProcessor;
        UsableField = usableField;
        SerializedFieldName = UsableField.Name;
        SerializedFieldInteropType = Il2CppInteropUtility.GetSerializedFieldInteropType(UsableField, Definitions);
        SerializedField = new FieldDefinition(
            SerializedFieldName,
            FieldAttributes.Public,
            Module.ImportReference(GetSerializedFieldInteropType())
        );
    }

    public void Process()
    {
        RenameUsableField();
        ComponentType.Fields.Add(SerializedField);
    }

    public void AddDeserializationIlCode(ILProcessor il)
    {
        var interopGetMethod = Module.ImportReference(SerializedFieldInteropType.Methods.First(x => x.Name == "Get"));
        if (SerializedFieldInteropType.HasGenericParameters)
        {
            interopGetMethod.DeclaringType = Module.ImportReference(
                SerializedFieldInteropType.MakeGenericInstanceType(UsableField.FieldType)
            );
        }

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, SerializedField);
        il.Emit(OpCodes.Callvirt, interopGetMethod);
        il.Emit(OpCodes.Stfld, UsableField);
    }

    private void RenameUsableField()
    {
        UsableField.Name = $"__{nameof(Il2CppAutoInterop)}_UsableField_{SerializedFieldName}";
    }

    private TypeReference GetSerializedFieldInteropType()
    {
        if (SerializedFieldInteropType.HasGenericParameters)
        {
            return SerializedFieldInteropType.CreateGenericInstanceType(UsableField.FieldType);
        }
        return SerializedFieldInteropType;
    }
}