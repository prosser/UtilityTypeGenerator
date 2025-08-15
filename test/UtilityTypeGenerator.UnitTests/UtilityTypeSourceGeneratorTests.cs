namespace UtilityTypeGenerator.UnitTests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit.Abstractions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

public class UtilityTypeSyntaxReceiverTests(ITestOutputHelper output)
{
    public static TheoryData<string, SyntaxNode, TypeDeclarationSyntax?, SyntaxKind, AttributeSyntax?> GetTestData()
    {
        TheoryData<string, SyntaxNode, TypeDeclarationSyntax?, SyntaxKind, AttributeSyntax?> data = [];

        foreach ((string typeName, string attributeJson) in new (string, string)[]
        {
            ("Picked", """
{
    "pick":{
        "type": "UtilityTypeGenerator.UnitTests.TestPoco",
        "properties":[
            "NotNullString"
        ]
    }
}
""")
        })
        {
            foreach (SyntaxKind typeKind in new[] { SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.RecordDeclaration, SyntaxKind.RecordStructDeclaration, SyntaxKind.InterfaceDeclaration })
            {
                AttributeSyntax attributeSyntax = CreateAttributeSyntax(attributeJson);
                CompilationUnitSyntax compilation = CreateUtilityTypePartialTypeForAugmentation(
                        typeKind,
                        typeName,
                        "TestNamespace",
                        attributeSyntax);

                data.Add(
                    typeName,
                    compilation.DescendantNodes().OfType<TypeDeclarationSyntax>().First(),
                    compilation.DescendantNodes().OfType<TypeDeclarationSyntax>().First(),
                    typeKind,
                    attributeSyntax);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(GetTestData))]
    public void OnVisitSyntaxNodeHandlesNodesProperly(string testName, SyntaxNode syntaxNode, TypeDeclarationSyntax? expectedTypeToAugment, SyntaxKind expectedSyntaxKind, AttributeSyntax? expectedUtilityTypeAttribute)
    {
        // arrange
        UtilityTypeSyntaxReceiver receiver = new();

        // act
        receiver.OnVisitSyntaxNode(syntaxNode);

        // assert
        receiver.Received.Should().ContainSingle();
        (TypeDeclarationSyntax typeToAugment, AttributeSyntax? utilityTypeAttribute) = receiver.Received[0];
        output.WriteLine(testName);
        typeToAugment.Should().Be(expectedTypeToAugment);
        typeToAugment.Kind().Should().Be(expectedSyntaxKind);

        if (expectedUtilityTypeAttribute is null)
        {
            utilityTypeAttribute.Should().BeNull();
        }
        else
        {
            utilityTypeAttribute.Should().NotBeNull();
            utilityTypeAttribute.ToFullString().Should().Be(expectedUtilityTypeAttribute.ToFullString());
        }
    }

    private static AttributeSyntax CreateAttributeSyntax(string attributeArg)
    {
        return Attribute(IdentifierName("UtilityType"))
            .WithArgumentList(
                AttributeArgumentList(SingletonSeparatedList(
                    AttributeArgument(
                        // pass a string literal expression as the argument
                        LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(attributeArg))))));
    }

    private static CompilationUnitSyntax CreateUtilityTypePartialTypeForAugmentation(
        SyntaxKind typeKind,
        string typeName,
        string typeNamespace,
        AttributeSyntax attribute,
        SyntaxKind accessibilityModifier = SyntaxKind.PublicKeyword,
        bool isReadOnly = false)
    {
        BaseTypeDeclarationSyntax typeDeclaration = typeKind switch
        {
            SyntaxKind.InterfaceDeclaration => InterfaceDeclaration(typeName),
            SyntaxKind.ClassDeclaration => ClassDeclaration(typeName),
            SyntaxKind.StructDeclaration => StructDeclaration(typeName),
            SyntaxKind.RecordDeclaration => RecordDeclaration(typeKind, Token(SyntaxKind.RecordKeyword), Identifier(typeName)),
            SyntaxKind.RecordStructDeclaration => RecordDeclaration(typeKind, Token(SyntaxKind.RecordKeyword), Identifier(typeName)),
            _ => throw new NotSupportedException($"Syntax kind {typeKind} is not supported")
        };

        SyntaxToken[] modifiers = [Token(accessibilityModifier)];
        if (isReadOnly)
        {
            modifiers = [.. modifiers, Token(SyntaxKind.ReadOnlyKeyword)];
        }

        modifiers = [.. modifiers, Token(SyntaxKind.PartialKeyword)];

        return CompilationUnit()
            .WithMembers(
                SingletonList<MemberDeclarationSyntax>(
                    // namespace {typeNamespace};
                    FileScopedNamespaceDeclaration(ParseName(typeNamespace))
                // {modifiers} {typeKind} {typeName};
                .WithMembers(SingletonList<MemberDeclarationSyntax>(
                    typeDeclaration
                    .WithAttributeLists(SingletonList(
                        AttributeList(SingletonSeparatedList(
                            // [UtilityType("{attributeArg}")]
                            attribute))))
                    .WithModifiers(TokenList(modifiers))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                ))
            ));
    }
}