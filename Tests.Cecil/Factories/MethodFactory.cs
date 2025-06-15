using Mono.Cecil;
using Mono.Cecil.Cil;
using Tests.Cecil.Common;

namespace Tests.Cecil.Factories;

internal static class MethodFactory
{
    public static MethodDefinition CreatePublicStaticVoidMethod(this TypeDefinition type, string name = Utility.TestMethodName)
    {
        const MethodAttributes attributes = MethodAttributes.Public | MethodAttributes.Static;
        var method = new MethodDefinition(name, attributes, type.Module.TypeSystem.Void);
        
        var il = method.Body.GetILProcessor();
        il.Emit(OpCodes.Ret);

        type.Methods.Add(method);
        return method;
    }

    public static FieldDefinition CreatePublicStaticStringField(this TypeDefinition type, string name = Utility.TestFieldName)
    {
        const FieldAttributes attributes = FieldAttributes.Public | FieldAttributes.Static;
        var field = new FieldDefinition(name, attributes, type.Module.TypeSystem.String);
        
        type.Fields.Add(field);
        return field;
    }
}