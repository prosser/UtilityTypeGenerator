namespace UtilityTypeGenerator;

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[Generator]
public sealed class UtilityTypeIncrementalSourceGenerator : IIncrementalGenerator
{
    private const string AttributeSource = """
namespace UtilityTypeGenerator
{
    using System;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    internal class UtilityTypeAttribute(string selector) : Attribute
    {
        public string Selector { get; } = selector;
    }
}
""";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Provide the attribute to the user compilation
        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource("UtilityTypeAttribute.g.cs", AttributeSource));

        // Find all type declarations annotated with our attribute
        IncrementalValuesProvider<Candidate> annotatedTypes = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "UtilityTypeGenerator.UtilityTypeAttribute",
            predicate: static (node, _) => node is TypeDeclarationSyntax,
            transform: static (ctx, ct) =>
            {
                var typeDecl = (TypeDeclarationSyntax)ctx.TargetNode;
                var typeSymbol = (INamedTypeSymbol)ctx.TargetSymbol;

                // get the attribute instance and its location/argument
                AttributeData attr = ctx.Attributes[0];
                string? selector = attr.ConstructorArguments.Length > 0
                    ? attr.ConstructorArguments[0].Value as string
                    : null;

                Location location = attr.ApplicationSyntaxReference?.GetSyntax(ct)?.GetLocation()
                    ?? typeDecl.GetLocation();

                string[] usingNamespaces = [];
                if (typeDecl.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault() is BaseNamespaceDeclarationSyntax containingNamespace)
                {
                    string[] nsArray =
                    [
                        containingNamespace.Name?.ToFullString() ?? "",
                        .. containingNamespace.Usings.Select(u => u.Name?.ToFullString() ?? "")
                    ];

                    usingNamespaces = [.. nsArray.Where(x => x.Length > 0).Distinct()];
                }

                return new Candidate(typeDecl, typeSymbol, selector, location, usingNamespaces);
            });

        // Combine with the compilation so we can resolve symbols/types
        IncrementalValuesProvider<(Candidate Left, Compilation Right)> withCompilation = annotatedTypes.Combine(context.CompilationProvider);

        IncrementalValuesProvider<GenerationResult> generationResults = withCompilation.Select(static (pair, ct) =>
        {
            ct.ThrowIfCancellationRequested();

            (Candidate candidate, Compilation compilation) = (pair.Left, pair.Right);
            string fileName = Path.GetFileNameWithoutExtension(candidate.Type.SyntaxTree.FilePath) + ".g.cs";

            // Validate inputs
            if (string.IsNullOrEmpty(candidate.Type.Identifier.Text) || string.IsNullOrEmpty(candidate.Selector))
            {
                return new GenerationResult(fileName, null, []);
            }

            try
            {
                Accessibility accessibility = candidate.Symbol.DeclaredAccessibility;

                UtilityTypeSelector? selector = UtilityTypeParser.Parse(candidate.UsingNamespaces, accessibility, compilation, candidate.Selector!);
                if (selector is null)
                {
                    var d = Diagnostic.Create(Diagnostics.InvalidSelector, candidate.AttributeLocation, candidate.Selector);
                    return new GenerationResult(fileName, null, [d]);
                }

                string? ns = candidate.Symbol.ContainingNamespace?.ToDisplayString();
                string source = UtilityTypeCSharpGenerator.Generate(
                    compilation,
                    selector,
                    candidate.Type.Kind(),
                    candidate.Type.Identifier.Text,
                    ns);

                return new GenerationResult(fileName, source, []);
            }
            catch (ArgumentException ex)
            {
                var d = Diagnostic.Create(Diagnostics.InvalidPropertyName, candidate.AttributeLocation, candidate.Selector, ex.Message);
                return new GenerationResult(fileName, null, [d]);
            }
            catch (FormatException ex)
            {
                var d = Diagnostic.Create(Diagnostics.InvalidSelector, candidate.AttributeLocation, candidate.Selector, ex.Message);
                return new GenerationResult(fileName, null, [d]);
            }
            catch (TypeLoadException ex)
            {
                var d = Diagnostic.Create(Diagnostics.TypeError, candidate.AttributeLocation, candidate.Selector, ex.Message);
                return new GenerationResult(fileName, null, [d]);
            }
            catch (InvalidOperationException ex)
            {
                var d = Diagnostic.Create(Diagnostics.TypeError, candidate.AttributeLocation, candidate.Selector, ex.Message);
                return new GenerationResult(fileName, null, [d]);
            }
            catch (Exception ex)
            {
                var d = Diagnostic.Create(Diagnostics.UnknownError, candidate.AttributeLocation, candidate.Selector, ex.ToString());
                return new GenerationResult(fileName, null, [d]);
            }
        });

        // Collect all results and emit one file per original file, merging blocks like the classic generator
        IncrementalValueProvider<ImmutableArray<GenerationResult>> collected = generationResults.Collect();
        context.RegisterSourceOutput(collected, static (spc, results) =>
        {
            StringBuilder sb = new StringBuilder()
                .AppendLine("// <auto-generated />")
                .Append("// generated at ").AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .AppendLine("#nullable enable")
                .AppendLine();
            string header = sb.ToString();

            foreach (IGrouping<string, GenerationResult>? group in results.GroupBy(r => r.FileName))
            {
                string[] blocks = [.. group
                    .Select(r => r.Source)
                    .Where(s => s is not null)
                    .Cast<string>()];

                if (blocks.Length > 0)
                {
                    spc.AddSource(group.Key, header + string.Join("\n", blocks));
                }
            }

            foreach (Diagnostic d in results.SelectMany(r => r.Diagnostics))
            {
                spc.ReportDiagnostic(d);
            }
        });
    }

    private readonly record struct Candidate(
        TypeDeclarationSyntax Type,
        INamedTypeSymbol Symbol,
        string? Selector,
        Location AttributeLocation,
        string[] UsingNamespaces);

    private readonly record struct GenerationResult(
        string FileName,
        string? Source,
        ImmutableArray<Diagnostic> Diagnostics);
}
