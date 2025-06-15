using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Il2CppAutoInterop.Cecil.Utils;

public static class MethodsUtility
{
    private static readonly OpCode[] FieldOpCodes = [OpCodes.Stfld, OpCodes.Ldfld, OpCodes.Stsfld, OpCodes.Ldsfld];
    private static readonly OpCode[] MethodOpCodes = [OpCodes.Call, OpCodes.Callvirt];
    
    public static string ParseTypeFullNameFromMethodFullName(string fullName)
    {
        if (fullName.Contains("::") && fullName.Contains(' '))
        {
            var items = fullName.Split("::");
            if (items.Length != 2)
            {
                throw new ArgumentException($"Unable to parse method full name: '{fullName}'");
            }

            var items2 = items[0].Split(" ");
            if (items2.Length < 2)
            {
                throw new ArgumentException($"Unable to parse return type on method method name: '{items[0]}'");
            }

            return items2.Last();
        }

        throw new ArgumentException($"Unable to find separated character in method full name: '{fullName}'");
    }

    public static bool IsFieldRelatedInstruction(Instruction instruction)
    {
        if (!FieldOpCodes.Contains(instruction.OpCode)) return false;
        return instruction.Operand is FieldReference;
    }

    public static bool HasFieldUsage(this MethodDefinition source, string targetFullName)
    {
        var instructions = source.Body.Instructions;
        foreach (var instruction in instructions)
        {
            if (!IsFieldRelatedInstruction(instruction)) continue;
            if (instruction.Operand is not FieldReference reference) continue;
            if (reference.FullName == targetFullName) return true;
        }
        return false;
    }
    
    public static bool HasMethodUsage(this MethodDefinition source, string targetFullName)
    {
        var instructions = source.Body.Instructions;
        foreach (var instruction in instructions)
        {
            if (!MethodOpCodes.Contains(instruction.OpCode)) continue;
            if (instruction.Operand is not MethodReference reference) continue;
            if (reference.FullName == targetFullName) return true;
        }
        return false;
    }
}