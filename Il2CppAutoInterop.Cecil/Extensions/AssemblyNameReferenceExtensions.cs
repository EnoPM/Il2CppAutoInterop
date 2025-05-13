using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Mono.Cecil;

namespace Il2CppAutoInterop.Cecil.Extensions;

public static class AssemblyNameReferenceExtensions
{
    public static bool TryResolveAssemblyName(
        this AssemblyNameReference reference,
        [NotNullWhen(true)] out AssemblyName? name)
    {
        try
        {
            name = new AssemblyName(reference.FullName);
            return true;
        }
        catch (Exception)
        {
            name = null;
            return false;
        }
    }
}