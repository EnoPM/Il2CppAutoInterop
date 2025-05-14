namespace Il2CppAutoInterop.Cecil.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class CecilResolveAttribute : Attribute
{
    public readonly string FullName;
    public readonly ResolverContext Context;

    public CecilResolveAttribute(string fullName, ResolverContext context = ResolverContext.All)
    {
        FullName = fullName;
        Context = context;
    }
}