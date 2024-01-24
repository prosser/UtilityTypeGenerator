namespace UtilityTypeGenerator;

using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.CodeAnalysis;
using UtilityTypeGenerator.Selectors;
using static UtilityTypeGenerator.UtilityTypesParser;

internal static class UtilityTypeHelper
{
    public static UtilityTypeSelector? Parse(string[] usingNamespaces, Compilation compilation, string input)
    {
        AntlrInputStream chars = new(input);
        UtilityTypesLexer lexer = new(chars);
        CommonTokenStream tokens = new(lexer);
        UtilityTypesParser parser = new(tokens)
        {
            BuildParseTree = true
        };

        IParseTree tree = parser.utility();
        UtilityTypeListener listener = new(usingNamespaces, compilation);
        ParseTreeWalker.Default.Walk(listener, tree);

        return listener.Selector;
    }
}

internal class UtilityTypeListener(string[] usingNamespaces, Compilation compilation) : UtilityTypesParserBaseListener
{
    public UtilityTypeSelector? Selector { get; private set; }

    public override void EnterUtility([NotNull] UtilityContext context)
    {
        if (Selector is not null)
        {
            // don't reparse descendants
            return;
        }

        Selector = ParseUtility(context);
    }

    private UtilityTypeSelector? ParseSelector<T, TContext>(UtilityContext context, Func<UtilityContext, SelectorContext?> getSelector, Func<UtilityContext, TContext?> getContext, Func<TContext, SelectorOrSymbol, T?> factory)
        where T : UtilityTypeSelector
    {
        if (getSelector(context) is not SelectorContext selectorCtx || getContext(context) is not TContext ctx)
        {
            return null;
        }

        SelectorOrSymbol sos = selectorCtx.utility() is UtilityContext utilityCtx
            ? new(ParseUtility(utilityCtx))
            : new(compilation.GetTypesByName(selectorCtx.GetText(), usingNamespaces).FirstOrDefault());

        return factory(ctx, sos);
    }

    private UtilityTypeSelector? ParseSelectorArray<T>(UtilityContext context, Func<UtilityContext, SelectorContext[]?> getSelectors, Func<IEnumerable<SelectorOrSymbol>, T?> factory)
        where T : UtilityTypeSelector
    {
        if (getSelectors(context) is not SelectorContext[] selectorContexts)
        {
            return null;
        }

        IEnumerable<SelectorOrSymbol> soss = selectorContexts
           .Select(x => x.GetText())
           .Select(x => new SelectorOrSymbol(compilation.GetTypesByName(x, usingNamespaces).FirstOrDefault()));

        return factory(soss);
    }

    private UtilityTypeSelector? ParseUtility(UtilityContext context)
    {
        return
            ParseSelector(context, x => x.pick()?.selector(), x => x.pick(), (s, sos) => new PickSelector(sos, s.property_name().Select(x => x.GetText()).ToArray()))
            ?? ParseSelector(context, x => x.omit()?.selector(), x => x.omit(), (s, sos) => new OmitSelector(sos, s.property_name().Select(x => x.GetText()).ToArray()))
            ?? ParseSelector(context, x => x.notnull()?.selector(), x => x.notnull(), (_, sos) => new NotNullSelector(sos))
            ?? ParseSelector(context, x => x.nullable()?.selector(), x => x.nullable(), (_, sos) => new NullableSelector(sos))
            ?? ParseSelector(context, x => x.optional()?.selector(), x => x.optional(), (_, sos) => new OptionalSelector(sos))
            ?? ParseSelector(context, x => x.import_selector()?.selector(), x => x.import_selector(), (_, sos) => new ImportSelector(sos))
            ?? ParseSelector(context, x => x.required()?.selector(), x => x.required(), (_, sos) => new RequiredSelector(sos))
            ?? ParseSelector(context, x => x.@readonly()?.selector(), x => x.@readonly(), (_, sos) => new ReadonlySelector(sos))
            ?? ParseSelectorArray(context, x => x.union()?.selector(), sos => new UnionSelector(sos))
            ?? ParseSelectorArray(context, x => x.intersection()?.selector(), sos => new IntersectionSelector(sos));
    }
}