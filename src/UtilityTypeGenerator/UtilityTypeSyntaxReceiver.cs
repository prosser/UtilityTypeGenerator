namespace UtilityTypeGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal class UtilityTypeSyntaxReceiver : ISyntaxReceiver
{
    public List<ReceivedSyntax> Received { get; } = [];

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        // the class to augment is a class that has the UtilityTypeAttribute
        if (syntaxNode is TypeDeclarationSyntax tds)
        {
            AttributeSyntax? attribute = tds.AttributeLists
                .SelectMany(al => al.Attributes)
                .FirstOrDefault(a => a.Name.ToString() is "UtilityTypeAttribute" or "UtilityType");

            if (attribute is not null)
            {
                Received.Add(new ReceivedSyntax(tds, attribute));
            }
        }
    }
}