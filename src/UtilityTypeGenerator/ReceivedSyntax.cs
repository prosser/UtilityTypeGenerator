namespace UtilityTypeGenerator;

using Microsoft.CodeAnalysis.CSharp.Syntax;

internal record ReceivedSyntax(TypeDeclarationSyntax TypeToAugment, AttributeSyntax UtilityTypeAttribute);