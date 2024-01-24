﻿namespace UtilityTypeGenerator;

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

        return """
// Generated by UtilityTypeGenerator
#nullable enable


""" + syntax.NormalizeWhitespace().ToFullString().Replace("null !", "default!");
    }

    private static SyntaxList<MemberDeclarationSyntax> Members(Compilation compilation, UtilityTypeSelector selector)
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

        foreach ((string propertyName, ITypeSymbol propertyType, bool nullable, bool isReadonly, bool isRequired) in selector.GetPropertyRecords(compilation))
        {
            string propertyTypeName = propertyType.ToDisplayString();

            PropertyDeclarationSyntax propertySyntax = PropertyDeclaration(
                    ParseTypeName(propertyTypeName),
                    Identifier(propertyName))
                .WithModifiers(GetPropertyModifiers(propertyType, isRequired))
                .WithAccessorList(GetAccessors(isRequired));

            if (!nullable && !propertyType.IsValueType && !isRequired)
            {
                propertySyntax = propertySyntax.WithInitializer(
                    EqualsValueClause(PostfixUnaryExpression(
                        SyntaxKind.SuppressNullableWarningExpression,
                        LiteralExpression(
                            SyntaxKind.NullLiteralExpression))))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
            }

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

        syntax = syntax
            .WithAttributeLists(TypeAttributes())
            .WithModifiers(TokenList([Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword)]));

        if (typeKind == SyntaxKind.RecordDeclaration)
        {
            syntax = syntax.WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken));
        }

        syntax = syntax.WithMembers(Members(compilation, selector));

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