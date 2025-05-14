namespace Il2CppAutoInterop.Cecil.Utils;

public static class MethodsUtility
{
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
}