using Il2CppAutoInterop.BepInEx.Contexts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mono.Cecil;
using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Il2CppAutoInterop.BepInEx.Utils;

public sealed class MonoBehaviourCSharpGenerator(BepInExPluginSerializationContext context, List<SerializedFieldGenerationData> serializedFields)
{
    private string BaseNamespace { get; } = context.ProcessingType.Namespace;
    private NameSyntax BaseNamespaceSyntax { get; } = CSharp.ParseName(context.ProcessingType.Namespace);
    private HashSet<string> UsingDirectives { get; } = [];

    public string GenerateFileContent()
    {
        var namespaceDeclarationSyntax = GetNamespaceDeclaration();

        var compilationUnitSyntax = CSharp.CompilationUnit()
            .AddUsings(GetUsingDirectiveDeclarations().ToArray())
            .AddMembers(namespaceDeclarationSyntax);

        return compilationUnitSyntax.NormalizeWhitespace().ToFullString();
    }

    private void RegisterUsingDirectiveType(TypeReference type)
    {
        UsingDirectives.UnionWith(GetRequiredUsings(type));
    }

    private HashSet<UsingDirectiveSyntax> GetUsingDirectiveDeclarations()
    {
        var usingDirectives = new HashSet<UsingDirectiveSyntax>();
        
        foreach (var ns in UsingDirectives)
        {
            if (ns == BaseNamespace) continue;
            var namespaceSyntax = CSharp.ParseName(ns);
            var usingDirective = CSharp.UsingDirective(namespaceSyntax);
            usingDirectives.Add(usingDirective);
        }
        
        return usingDirectives;
    }

    private NamespaceDeclarationSyntax GetNamespaceDeclaration()
    {
        var typeDeclaration = GetTypeDeclaration();
        
        var namespaceDeclaration = CSharp.NamespaceDeclaration(BaseNamespaceSyntax)
            .AddMembers(typeDeclaration);

        return namespaceDeclaration;
    }

    private ClassDeclarationSyntax GetTypeDeclaration()
    {
        var members = new List<MemberDeclarationSyntax>();
        
        members.AddRange(GetSerializedFieldDeclarations());
        members.AddRange(GetSerializedMethodDeclarations());
        
        var baseType = GetTypeSyntax(context.ProcessingType.BaseType);
        RegisterUsingDirectiveType(context.ProcessingType.BaseType);

        var classDeclaration = CSharp.ClassDeclaration(context.ProcessingType.Name)
            .AddModifiers(CSharp.Token(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(CSharp.SimpleBaseType(baseType))
            .AddMembers(members.ToArray());

        if (context.ProcessingType.IsAbstract)
        {
            classDeclaration = classDeclaration.AddModifiers(CSharp.Token(SyntaxKind.AbstractKeyword));
        }
        else if (context.ProcessingType.IsSealed)
        {
            classDeclaration = classDeclaration.AddModifiers(CSharp.Token(SyntaxKind.SealedKeyword));
        }

        return classDeclaration;
    }
    
    private FieldDeclarationSyntax[] GetSerializedFieldDeclarations()
    {
        var fields = new List<FieldDeclarationSyntax>();

        foreach (var field in serializedFields)
        {
            var isPluginMonoBehaviourFieldType = Il2CppInteropUtility.IsPluginMonoBehaviourFieldType(field.UsableField, context);
            var usableFieldType = isPluginMonoBehaviourFieldType ? context.InteropTypes.GameObjectType.Value : field.UsableField.FieldType;
            var fieldTypeSyntax = GetTypeSyntax(usableFieldType);
            var fieldDeclarationSyntax = CSharp.FieldDeclaration(
                    CSharp.VariableDeclaration(fieldTypeSyntax)
                        .AddVariables(CSharp.VariableDeclarator(field.SerializedField.Name)))
                .WithModifiers(CSharp.TokenList(CSharp.Token(SyntaxKind.PublicKeyword)));
            fields.Add(fieldDeclarationSyntax);
            RegisterUsingDirectiveType(field.UsableField.FieldType);
        }

        return fields.ToArray();
    }

    private MethodDeclarationSyntax[] GetSerializedMethodDeclarations()
    {
        var methods = new List<MethodDeclarationSyntax>();

        foreach (var method in context.ProcessingType.Methods)
        {
            if (method.IsConstructor || method.IsStatic || !method.IsPublic) continue;
            if (method.HasParameters || method.HasGenericParameters) continue;
            if (method.ReturnType.FullName != context.ProcessingModule.TypeSystem.Void.FullName) continue;
            var modifiers = new List<SyntaxToken>
            {
                CSharp.Token(SyntaxKind.PublicKeyword)
            };

            if (method.IsAbstract)
            {
                modifiers.Add(CSharp.Token(SyntaxKind.AbstractKeyword));
            }
            else if (method.IsVirtual && method.IsReuseSlot && !method.IsNewSlot)
            {

                modifiers.Add(CSharp.Token(SyntaxKind.OverrideKeyword));
                if (method.IsFinal)
                {
                    modifiers.Insert(0, CSharp.Token(SyntaxKind.SealedKeyword));
                }
            }
            else if (method.IsVirtual && method.IsNewSlot)
            {
                modifiers.Add(CSharp.Token(SyntaxKind.VirtualKeyword));
            }
            else if (!method.IsVirtual && method.IsReuseSlot && method.IsNewSlot)
            {
                modifiers.Add(CSharp.Token(SyntaxKind.NewKeyword));
            }

            var methodDecl = CSharp.MethodDeclaration(
                    CSharp.PredefinedType(CSharp.Token(SyntaxKind.VoidKeyword)),
                    CSharp.Identifier(method.Name))
                .WithModifiers(CSharp.TokenList(modifiers));

            if (method.IsAbstract)
            {
                methodDecl = methodDecl.WithSemicolonToken(CSharp.Token(SyntaxKind.SemicolonToken));
            }
            else
            {
                methodDecl = methodDecl.WithBody(CSharp.Block());
            }

            methods.Add(methodDecl);
        }
        
        return methods.ToArray();
    }

    private static TypeSyntax GetTypeSyntax(TypeReference? typeReference)
    {
        if (typeReference == null)
        {
            return CSharp.PredefinedType(CSharp.Token(SyntaxKind.VoidKeyword));
        }

        var typeName = typeReference.FullName.Replace("/", ".");
        var ns = typeReference.Namespace;
        if (!string.IsNullOrEmpty(ns) && typeName.StartsWith($"{ns}."))
        {
            typeName = typeName[$"{ns}.".Length..];
        }

        if (typeReference is GenericInstanceType genericInstance)
        {
            var genericName = CSharp.GenericName(CSharp.Identifier(typeReference.Name[..typeReference.Name.IndexOf('`')]));
            var typeArguments = genericInstance.GenericArguments.Select(GetTypeSyntax);
            genericName = genericName.AddTypeArgumentListArguments(typeArguments.ToArray());
            return genericName;
        }

        return CSharp.ParseTypeName(typeName);
    }

    private static IEnumerable<string> GetRequiredUsings(TypeReference typeReference)
    {
        var namespaces = new HashSet<string>();

        if (typeReference.IsPrimitive)
        {
            return namespaces;
        }

        if (!string.IsNullOrEmpty(typeReference.Namespace))
        {
            namespaces.Add(typeReference.Namespace);
        }

        if (typeReference is GenericInstanceType genericInstance)
        {
            foreach (var arg in genericInstance.GenericArguments)
            {
                namespaces.UnionWith(GetRequiredUsings(arg));
            }
        }

        return namespaces;
    }
}