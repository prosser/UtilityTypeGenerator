namespace UtilityTypeGenerator;

using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[Generator]
public sealed class UtilityTypeSourceGenerator : ISourceGenerator
{
    private const string AttributeSource = """
namespace UtilityTypeGenerator
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class UtilityTypeAttribute(string selector) : Attribute
    {
        public string Selector { get; } = selector;
    }
}
""";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1075:Avoid empty catch clause that catches System.Exception", Justification = "<Pending>")]
    public void Execute(GeneratorExecutionContext context)
    {
        // always add the attribute source. This is a semi-magical incantation that makes attributes on the generator work.
        context.AddSource("UtilityTypeAttribute.g.cs", AttributeSource);

        if (context.SyntaxReceiver is not UtilityTypeSyntaxReceiver receiver ||
            receiver.Received.Count == 0)
        {
            return;
        }

        StringBuilder sb = new();
        string? baseName = null;
        foreach ((TypeDeclarationSyntax typeSyntax, AttributeSyntax attribute) in receiver.Received)
        {
            string typeName = typeSyntax.Identifier.Text;
            if (string.IsNullOrEmpty(typeName))
            {
                continue;
            }

            // read the UtilityTypeAttribute from the class to augment
            if (attribute
                    .DescendantNodes()
                    .OfType<LiteralExpressionSyntax>()
                    .FirstOrDefault()
                    ?.Token.ValueText is not string selectorSyntax)
            {
                continue;
            }

            try
            {
                //Debugger.Launch();

                UtilityTypeSelector? selector = UtilityTypeHelper.Parse([], context.Compilation, selectorSyntax);
                if (selector is null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.InvalidSelector, attribute.GetLocation(), selectorSyntax));
                    continue;
                }

                // get the namespace for the class
                string? @namespace = context.Compilation.GetSemanticModel(typeSyntax.SyntaxTree).GetDeclaredSymbol(typeSyntax)?.ContainingNamespace?.ToString();

                sb = sb.AppendLine(UtilityTypeCSharpGenerator.Generate(context.Compilation, selector, typeSyntax.Kind(), typeName, @namespace));
                baseName = Path.GetFileNameWithoutExtension(typeSyntax.SyntaxTree.FilePath);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        if (sb.Length > 0 && baseName is not null)
        {
            // get the file name containing the class to augment
            context.AddSource($"{baseName}.g.cs", sb.ToString());
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new UtilityTypeSyntaxReceiver());
    }
}
