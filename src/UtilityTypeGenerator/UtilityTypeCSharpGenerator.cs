namespace UtilityTypeGenerator;

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

internal static class UtilityTypeCSharpGenerator
{
    public static string Generate(Compilation compilation, UtilityTypeSelector selector, SyntaxKind typeKind, string name, string? @namespace)
    {
        // find the syntax tree for the type, using the full type name in case of duplicate type names in different namespaces
        CompilationUnitSyntax syntax = CompilationUnit();

        syntax = @namespace is not null
            ? syntax.WithMembers(
                SingletonList<MemberDeclarationSyntax>(
                    NamespaceDeclaration(ParseName(@namespace))
                    .WithUsings(UsingDirectives())
                    .WithMembers(
                        SingletonList<MemberDeclarationSyntax>(
                            Type(compilation, typeKind, name, selector)))))
            : syntax.WithUsings(UsingDirectives())
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        Type(compilation, typeKind, name, selector)));

        return syntax.NormalizeWhitespace().ToFullString().Replace("null !", "default!");
    }

    private static SyntaxList<MemberDeclarationSyntax> Members(Compilation compilation, SyntaxKind targetKind, UtilityTypeSelector selector)
    {
        SyntaxList<MemberDeclarationSyntax> list = [];

        static SyntaxTokenList GetPropertyModifiers(ITypeSymbol propertyType, bool isRequired)
        {
            List<SyntaxToken> tokens = [];
            switch (propertyType.DeclaredAccessibility)
            {
                case Accessibility.Public:
                    tokens.Add(Token(SyntaxKind.PublicKeyword));
                    break;

                case Accessibility.Protected:
                    tokens.Add(Token(SyntaxKind.ProtectedKeyword));
                    break;

                case Accessibility.Private:
                    tokens.Add(Token(SyntaxKind.PrivateKeyword));
                    break;

                case Accessibility.Internal:
                    tokens.Add(Token(SyntaxKind.InternalKeyword));
                    break;
            }

            if (isRequired)
            {
                tokens.Add(Token(SyntaxKind.RequiredKeyword));
            }

            return TokenList(tokens);
        }

        static AccessorListSyntax GetAccessors(bool isRequired)
        {
            return isRequired
                ? AccessorList(List(
                    [
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                        AccessorDeclaration(SyntaxKind.InitAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                    ]))
                : AccessorList(List(
                    [
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                        AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                    ]));
        }

        foreach ((ITypeSymbol containingType, string propertyName, ITypeSymbol propertyType, bool nullable, bool isReadonly, bool isRequired) in selector.GetPropertyRecords(compilation))
        {
            // find the syntax tree for the property's declaring type, using the full type name in case of duplicate type names in different namespaces
            SyntaxTree containingTypeSyntax = compilation.SyntaxTrees
                .Where(tree => tree.FilePath == containingType.Locations[0].SourceTree?.FilePath)
                .Select(tree => tree.GetRoot())
                .OfType<CompilationUnitSyntax>()
                .SelectMany(syntax => syntax.DescendantNodes().OfType<TypeDeclarationSyntax>())
                .Where(type => type.Identifier.ValueText == containingType.Name)
                .Select(type => type.SyntaxTree)
                .FirstOrDefault();

            PropertyDeclarationSyntax? sourcePropertySyntax = containingTypeSyntax?.GetRoot()
                .DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault(property => property.Identifier.ValueText == propertyName);

            SyntaxTriviaList? leadingTrivias = null;
            if (sourcePropertySyntax is not null)
            {
                // get the leading trivia for the property
                leadingTrivias = sourcePropertySyntax.GetLeadingTrivia();
            }

            string propertyTypeName = propertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (nullable && !propertyTypeName.EndsWith("?"))
            {
                propertyTypeName += "?";
            }

            PropertyDeclarationSyntax propertySyntax = PropertyDeclaration(
                    ParseTypeName(propertyTypeName),
                    Identifier(propertyName))
                .WithModifiers(GetPropertyModifiers(propertyType, isRequired))
                .WithAccessorList(GetAccessors(isRequired));

            if (leadingTrivias is not null)
            {
                propertySyntax = propertySyntax.WithLeadingTrivia(leadingTrivias);
            }

            if (targetKind != SyntaxKind.InterfaceDeclaration && !nullable && !propertyType.IsValueType && !isRequired)
            {
                propertySyntax = propertySyntax.WithInitializer(
                    EqualsValueClause(PostfixUnaryExpression(
                        SyntaxKind.SuppressNullableWarningExpression,
                        LiteralExpression(
                            SyntaxKind.NullLiteralExpression))))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
            }

#if DEBUG
            SyntaxTriviaList diagComment = TriviaList(Comment($"// targetKind={targetKind}, nullable={nullable}, isValueType={propertyType.IsValueType}, isRequired={isRequired}"));
            propertySyntax = propertySyntax.WithTrailingTrivia(diagComment);
#endif
            list = list.Add(propertySyntax);
        }

        return list;
    }

    private static TypeDeclarationSyntax Type(Compilation compilation, SyntaxKind typeKind, string name, UtilityTypeSelector selector)
    {
        TypeDeclarationSyntax syntax = typeKind switch
        {
            SyntaxKind.ClassDeclaration => ClassDeclaration(name),
            SyntaxKind.StructDeclaration => StructDeclaration(name),
            SyntaxKind.InterfaceDeclaration => InterfaceDeclaration(name),
            SyntaxKind.RecordDeclaration => RecordDeclaration(
                typeKind,
                Token(SyntaxKind.RecordKeyword),
                Identifier(name)),
            _ => throw new ArgumentException($"Invalid type kind: {typeKind}", nameof(typeKind))
        };

        List<SyntaxToken> modifiers = [
            .. selector.Accessibility switch
            {
                Accessibility.Public => (SyntaxToken[])[Token(SyntaxKind.PublicKeyword)],
                Accessibility.Protected => [Token(SyntaxKind.ProtectedKeyword)],
                Accessibility.Private => [Token(SyntaxKind.PrivateKeyword)],
                Accessibility.ProtectedAndInternal => [Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.InternalKeyword)],
                _ => [Token(SyntaxKind.InternalKeyword)],
            },
            Token(SyntaxKind.PartialKeyword)
        ];

        syntax = syntax
            .WithAttributeLists(TypeAttributes())
            .WithModifiers(TokenList(modifiers));

        if (typeKind == SyntaxKind.RecordDeclaration)
        {
            syntax = syntax.WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken));
        }

        syntax = syntax.WithMembers(Members(compilation, typeKind, selector));

        if (typeKind == SyntaxKind.RecordDeclaration)
        {
            syntax = syntax.WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken));
        }

        return syntax;
    }

    private static SyntaxList<AttributeListSyntax> TypeAttributes()
    {
        return SingletonList(
            AttributeList(
                SingletonSeparatedList(
                    Attribute(IdentifierName("GeneratedCode"))
                        .WithArgumentList(
                            AttributeArgumentList(
                                SeparatedList<AttributeArgumentSyntax>(
                                    new SyntaxNodeOrToken[]
                                    {
                                        AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("UtilityTypeGenerator"))),
                                        Token(SyntaxKind.CommaToken),
                                        AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("1.0.0")))
                                    }))))));
    }

    private static SyntaxList<UsingDirectiveSyntax> UsingDirectives()
    {
        return SingletonList(
            UsingDirective(
                QualifiedName(
                    QualifiedName(IdentifierName("System"), IdentifierName("CodeDom")),
                    IdentifierName("Compiler"))));
    }
}