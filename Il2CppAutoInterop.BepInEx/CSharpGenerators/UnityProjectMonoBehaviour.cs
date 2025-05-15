using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Il2CppAutoInterop.BepInEx.Processors.TypeProcessors;
using Mono.Cecil;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Il2CppAutoInterop.BepInEx.CSharpGenerators;

public sealed class UnityProjectMonoBehaviour(TypeDefinition type, SerializedMonoBehaviourProcessor processor)
{
    public string GenerateFileContent()
    {
        var usings = new HashSet<string>();
        var baseNamespace = type.Namespace;

        var fields = new List<MemberDeclarationSyntax>();

        foreach (var field in processor.SerializedFields)
        {
            var fieldTypeSyntax = GetTypeSyntax(field.UsableField.FieldType);
            var fieldDeclarationSyntax = FieldDeclaration(
                    VariableDeclaration(fieldTypeSyntax)
                        .AddVariables(VariableDeclarator(field.SerializedField.Name)))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)));
            fields.Add(fieldDeclarationSyntax);
            usings.UnionWith(GetRequiredUsings(field.UsableField.FieldType));
        }

        var baseTypeSyntax = GetTypeSyntax(type.BaseType);
        usings.UnionWith(GetRequiredUsings(type.BaseType));

        var classDeclarationSyntax = ClassDeclaration(type.Name)
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(SimpleBaseType(baseTypeSyntax))
            .AddMembers(fields.ToArray());

        if (type.IsAbstract)
        {
            classDeclarationSyntax = classDeclarationSyntax.AddModifiers(Token(SyntaxKind.AbstractKeyword));
        }
        else if (type.IsSealed)
        {
            classDeclarationSyntax = classDeclarationSyntax.AddModifiers(Token(SyntaxKind.SealedKeyword));
        }

        var namespaceDeclarationSyntax = NamespaceDeclaration(ParseName(baseNamespace)).AddMembers(classDeclarationSyntax);

        var compilationUnitSyntax = CompilationUnit()
            .AddUsings(usings.Where(x => x != baseNamespace).Select(x => UsingDirective(ParseName(x))).ToArray())
            .AddMembers(namespaceDeclarationSyntax);

        return compilationUnitSyntax.NormalizeWhitespace().ToFullString();
    }
    
    private static TypeSyntax GetTypeSyntax(TypeReference? typeReference)
    {
        if (typeReference == null)
        {
            return PredefinedType(Token(SyntaxKind.VoidKeyword));
        }

        var typeName = typeReference.FullName.Replace("/", ".");
        var ns = typeReference.Namespace;
        if (!string.IsNullOrEmpty(ns) && typeName.StartsWith($"{ns}."))
        {
            typeName = typeName[$"{ns}.".Length..];
        }

        if (typeReference is GenericInstanceType genericInstance)
        {
            var genericName = GenericName(Identifier(typeReference.Name[..typeReference.Name.IndexOf('`')]));
            var typeArguments = genericInstance.GenericArguments.Select(GetTypeSyntax);
            genericName = genericName.AddTypeArgumentListArguments(typeArguments.ToArray());
            return genericName;
        }

        return ParseTypeName(typeName);
    }

    private static IEnumerable<string> GetRequiredUsings(TypeReference typeReference)
    {
        var namespaces = new HashSet<string>();

        if (typeReference.IsPrimitive)
            return namespaces;

        if (!string.IsNullOrEmpty(typeReference.Namespace))
            namespaces.Add(typeReference.Namespace);

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