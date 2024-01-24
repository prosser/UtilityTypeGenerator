namespace UtilityTypeGenerator;

using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

//[Generator]
//public sealed class UtilityTypeIncrementalGenerator : IIncrementalGenerator
//{
//    public void Initialize(IncrementalGeneratorInitializationContext context)
//    {
//        // find all additional files that end with ".utility";
//        IncrementalValuesProvider<AdditionalText> utilityFiles = context.AdditionalTextsProvider.Where(
//            static file => file.Path.EndsWith(".utility"));

//        // read their contents and save their name
//        IncrementalValuesProvider<(string name, string content)> namesAndContents = utilityFiles
//            .Select((text, ct) => (name: Path.GetFileNameWithoutExtension(text.Path), content: text.GetText(ct)!.ToString()));

//        // generate types from the contents
//    }
//}

[Generator]
public sealed class UtilityTypeSourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        string id = DateTime.Now.ToString("hh:mm:ss");
        DebugLog($"{id} Execute");
        if (context.SyntaxReceiver is not UtilityTypeSyntaxReceiver receiver ||
            receiver.Received.Count == 0)
        {
            return;
        }

        StringBuilder sb = new();
        string? baseName = null;
        foreach ((TypeDeclarationSyntax typeSyntax, SyntaxKind kind, AttributeSyntax attribute) in receiver.Received)
        {
            string typeName = typeSyntax.Identifier.Text;
            if (string.IsNullOrEmpty(typeName))
            {
                DebugLog($"{id} typeName is null");
                continue;
            }

            // read the UtilityTypeAttribute from the class to augment
            DebugLog(id + " " + attribute.ToFullString());
            if (attribute
                    .DescendantNodes()
                    .OfType<LiteralExpressionSyntax>()
                    .FirstOrDefault()
                    ?.Token.ValueText is not string selectorSyntax)
            {
                DebugLog($"{id} selectorSyntax is not a string");
                continue;
            }

            try
            {
                //Debugger.Launch();

                UtilityTypeSelector? selector = UtilityTypeHelper.Parse([], context.Compilation, selectorSyntax);
                if (selector is null)
                {
                    DebugLog($"{id} selectorSyntax failed parsing");
                    context.AddSource($"{typeName}.UtilityType.g.cs", $"/* Error parsing selector: {selectorSyntax} */");
                    continue;
                }

                // get the namespace for the class
                string? @namespace = context.Compilation.GetSemanticModel(typeSyntax.SyntaxTree).GetDeclaredSymbol(typeSyntax)?.ContainingNamespace?.ToString();

                sb = sb.AppendLine(UtilityTypeCSharpGenerator.Generate(context.Compilation, selector, kind, typeName, @namespace));
                baseName = Path.GetFileNameWithoutExtension(typeSyntax.SyntaxTree.FilePath);
            }
            catch (Exception ex)
            {
                DebugLog($"{id} exception for {typeName}: {ex}");
            }
        }

        if (sb.Length > 0 && baseName is not null)
        {
            // get the file name containing the class to augment

            DebugLog($"{id} source: {sb}");
            context.AddSource($"{baseName}.UtilityType.g.cs", sb.ToString());
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new UtilityTypeSyntaxReceiver());
    }

    [Conditional("Debug")]
    private static void DebugLog(string message)
    {
        Debug.WriteLine(message);
    }
}

internal record SyntaxReceived(TypeDeclarationSyntax TypeToAugment, SyntaxKind SyntaxKind, AttributeSyntax UtilityTypeAttribute);

internal class UtilityTypeSyntaxReceiver : ISyntaxReceiver
{
    public List<SyntaxReceived> Received { get; } = [];

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        string id = DateTime.Now.ToString("hh:mm:ss");
        // the class to augment is a class that has the UtilityTypeAttribute
        if (syntaxNode is TypeDeclarationSyntax tds)
        {
            AttributeSyntax? attribute = tds.AttributeLists
                .SelectMany(al => al.Attributes)
                .FirstOrDefault(a => a.Name.ToString() is "UtilityTypeAttribute" or "UtilityType");

            if (attribute is not null)
            {
                //TypeToAugment = tds;
                Received.Add(new SyntaxReceived(tds, syntaxNode.Kind(), attribute));
            }
        }
    }
}