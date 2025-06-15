using Il2CppAutoInterop.BepInEx.Contexts;
using Il2CppAutoInterop.Common.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mono.Cecil;
using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Il2CppAutoInterop.BepInEx.Utils;

public sealed class MonoBehaviourCSharpGenerator
{
    private readonly BepInExPluginSerializationContext _context;
    private readonly List<SerializedFieldGenerationData> _serializedFields;
    private string BaseNamespace { get; }
    private NameSyntax BaseNamespaceSyntax { get; }
    private HashSet<string> UsingDirectives { get; } = [];

    private Dictionary<string, PredefinedTypeSyntax> PredefinedTypes { get; }

    public MonoBehaviourCSharpGenerator(BepInExPluginSerializationContext context, List<SerializedFieldGenerationData> serializedFields)
    {
        _context = context;
        _serializedFields = serializedFields;
        BaseNamespace = context.ProcessingType.Namespace;
        BaseNamespaceSyntax = CSharp.ParseName(context.ProcessingType.Namespace);

        var system = context.ProcessingModule.TypeSystem;

        PredefinedTypes = new Dictionary<string, PredefinedTypeSyntax>
        {
            [system.Boolean.FullName] = PredefinedType(SyntaxKind.BoolKeyword),
            [system.Byte.FullName] = PredefinedType(SyntaxKind.ByteKeyword),
            [system.SByte.FullName] = PredefinedType(SyntaxKind.SByteKeyword),
            [system.Char.FullName] = PredefinedType(SyntaxKind.CharKeyword),
            [system.Double.FullName] = PredefinedType(SyntaxKind.DoubleKeyword),
            [system.Single.FullName] = PredefinedType(SyntaxKind.FloatKeyword),
            [system.Int32.FullName] = PredefinedType(SyntaxKind.IntKeyword),
            [system.UInt32.FullName] = PredefinedType(SyntaxKind.UIntKeyword),
            [system.Int64.FullName] = PredefinedType(SyntaxKind.LongKeyword),
            [system.UInt64.FullName] = PredefinedType(SyntaxKind.ULongKeyword),
            [system.Int16.FullName] = PredefinedType(SyntaxKind.ShortKeyword),
            [system.UInt16.FullName] = PredefinedType(SyntaxKind.UShortKeyword),
            [system.Object.FullName] = PredefinedType(SyntaxKind.ObjectKeyword),
            [system.String.FullName] = PredefinedType(SyntaxKind.StringKeyword),
            [system.Void.FullName] = PredefinedType(SyntaxKind.VoidKeyword),
        };
    }

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

        var baseType = GetTypeSyntax(_context.ProcessingType.BaseType);
        RegisterUsingDirectiveType(_context.ProcessingType.BaseType);

        var classDeclaration = CSharp.ClassDeclaration(_context.ProcessingType.Name)
            .AddModifiers(CSharp.Token(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(CSharp.SimpleBaseType(baseType))
            .AddMembers(members.ToArray());

        if (_context.ProcessingType.IsAbstract)
        {
            classDeclaration = classDeclaration.AddModifiers(CSharp.Token(SyntaxKind.AbstractKeyword));
        }
        else if (_context.ProcessingType.IsSealed)
        {
            classDeclaration = classDeclaration.AddModifiers(CSharp.Token(SyntaxKind.SealedKeyword));
        }

        return classDeclaration;
    }

    private FieldDeclarationSyntax[] GetSerializedFieldDeclarations()
    {
        var fields = new List<FieldDeclarationSyntax>();

        foreach (var field in _serializedFields)
        {
            var isPluginMonoBehaviourFieldType = Il2CppInteropUtility.IsPluginMonoBehaviourFieldType(field.UsableField, _context);
            var usableFieldType = isPluginMonoBehaviourFieldType ? _context.InteropTypes.GameObjectType.Value : field.UsableField.FieldType;
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

        foreach (var method in _context.ProcessingType.Methods)
        {
            if (method.IsConstructor || method.IsStatic || !method.IsPublic || method.IsGetter || method.IsSetter) continue;
            if (method.HasGenericParameters) continue;

            if (method.Parameters.Any(p => !IsManagedSimpleType(p.ParameterType)))
            {
                var allParameters = method.Parameters.Select(x => $"'{x.ParameterType.FullName}'").ToArray();
                Logger.Instance.Warning($"Method {method.Name} has unsupported parameter type {string.Join(", ", allParameters)}");
                continue;
            }
            if (!IsManagedSimpleType(method.ReturnType))
            {
                Logger.Instance.Warning($"Method {method.Name} has unsupported return type '{method.ReturnType.FullName}'");
                continue;
            }

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

            var returnType = PredefinedTypes.TryGetValue(method.ReturnType.FullName, out var predefinedReturnType)
                ? predefinedReturnType
                : GetTypeSyntax(method.ReturnType);
            
            var parameters = new List<ParameterSyntax>();

            foreach (var parameterDefinition in method.Parameters)
            {
                var typeSyntax = PredefinedTypes.TryGetValue(parameterDefinition.ParameterType.FullName, out var predefinedTypeSyntax)
                    ? predefinedTypeSyntax
                    : GetTypeSyntax(parameterDefinition.ParameterType);
                var parameter = CSharp.Parameter(CSharp.Identifier(parameterDefinition.Name))
                    .WithType(typeSyntax);
                
                parameters.Add(parameter);
            }

            var methodDecl = CSharp.MethodDeclaration(returnType, CSharp.Identifier(method.Name))
                .WithModifiers(CSharp.TokenList(modifiers))
                .WithParameterList(CSharp.ParameterList(CSharp.SeparatedList(parameters)));

            if (method.IsAbstract)
            {
                methodDecl = methodDecl.WithSemicolonToken(CSharp.Token(SyntaxKind.SemicolonToken));
            }
            else
            {
                if (method.ReturnType.FullName == _context.ProcessingModule.TypeSystem.Void.FullName)
                {
                    methodDecl = methodDecl.WithBody(CSharp.Block());
                }
                else
                {
                    methodDecl = methodDecl.WithBody(CSharp.Block(
                        CSharp.SingletonList<StatementSyntax>(
                            CSharp.ReturnStatement(CSharp.LiteralExpression(SyntaxKind.DefaultLiteralExpression))
                        )
                    ));
                }
            }

            methods.Add(methodDecl);
        }

        return methods.ToArray();
    }

    private bool IsManagedSimpleType(TypeReference type)
    {
        return PredefinedTypes.ContainsKey(type.FullName);
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

    private static PredefinedTypeSyntax PredefinedType(SyntaxKind syntaxKind)
    {
        return CSharp.PredefinedType(CSharp.Token(syntaxKind));
    }

}