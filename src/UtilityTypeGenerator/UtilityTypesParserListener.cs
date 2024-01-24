namespace UtilityTypeGenerator;

using Antlr4.Runtime.Misc;
using Microsoft.CodeAnalysis;
using UtilityTypeGenerator.Selectors;
using static UtilityTypeGenerator.UtilityTypesParser;

internal class UtilityTypesParserListener(string[] usingNamespaces, Compilation compilation) : UtilityTypesParserBaseListener
{
    public UtilityTypeSelector? Selector { get; private set; }

    public override void EnterSelector([NotNull] SelectorContext context)
    {
        if (Selector is not null)
        {
            // don't reparse descendants
            return;
        }

        Selector = ParseSelector(context);
    }

    private static string GetQuotedOrUnquotedIdText(Property_nameContext context)
    {
        Quoted_or_unquoted_idContext ctx = context.quoted_or_unquoted_id();
        return ctx.quoted_id() is Quoted_idContext quotedId
            ? quotedId.GetText()[1..^1]
            : ctx.unquoted_id().GetText();
    }

    private UtilityTypeSelector? ParseSelector(SelectorContext context)
    {
        if (context.has_props() is Has_propsContext props &&
            props.has_props_verb()?.GetText() is string propsVerb)
        {
            return ParseSelectorWithTypeAndPropertiesArgs(usingNamespaces, compilation, props, propsVerb);
        }

        if (context.has_type() is Has_typeContext type &&
            type.has_type_verb()?.GetText() is string typeVerb)
        {
            return ParseSelectorWithSingleTypeArg(usingNamespaces, compilation, type, typeVerb);
        }

        return context.has_types() is Has_typesContext types &&
            types.has_types_verb()?.GetText() is string typesVerb
            ? ParseSelectorWithMultipleTypeArgs(usingNamespaces, compilation, types, typesVerb)
            : throw new FormatException("Unknown verb");
    }

    private UtilityTypeSelector ParseSelectorWithMultipleTypeArgs(string[] usingNamespaces, Compilation compilation, Has_typesContext types, string typesVerb)
    {
        if (types.exception is not null)
        {
            throw new FormatException(types.exception.Message);
        }

        SymbolOrSelector[] sos = types.symbol_or_selector()
            .Select(ctx => ParseSymbolOrSelector(ctx, usingNamespaces, compilation))
            .ToArray();

        return typesVerb switch
        {
            "Intersect" => new IntersectionSelector(sos),
            "Intersection" => new IntersectionSelector(sos),
            "Union" => new UnionSelector(sos),
            _ => throw new FormatException($"Unknown verb: {typesVerb}")
        };
    }

    private UtilityTypeSelector ParseSelectorWithSingleTypeArg(string[] usingNamespaces, Compilation compilation, Has_typeContext type, string typeVerb)
    {
        if (type.exception is not null)
        {
            throw new FormatException(type.exception.Message);
        }

        SymbolOrSelector sos = ParseSymbolOrSelector(type.symbol_or_selector(), usingNamespaces, compilation);

        return typeVerb switch
        {
            "Import" => new ImportSelector(sos),
            "NotNull" => new NotNullSelector(sos),
            "Nullable" => new NullableSelector(sos),
            "Optional" => new OptionalSelector(sos),
            "Readonly" => new ReadonlySelector(sos),
            "Required" => new RequiredSelector(sos),
            _ => throw new FormatException($"Unknown verb: {typeVerb}")
        };
    }

    private UtilityTypeSelector ParseSelectorWithTypeAndPropertiesArgs(string[] usingNamespaces, Compilation compilation, Has_propsContext props, string propsVerb)
    {
        if (props.exception is not null)
        {
            throw new FormatException(props.exception.Message);
        }

        SymbolOrSelector sos = ParseSymbolOrSelector(props.symbol_or_selector(), usingNamespaces, compilation);

        string[] propertyNames = props.property_name().Select(GetQuotedOrUnquotedIdText).ToArray();

        return propsVerb switch
        {
            "Pick" => new PickSelector(sos, propertyNames),
            "Omit" => new OmitSelector(sos, propertyNames),
            _ => throw new FormatException($"Unknown verb: {propsVerb}")
        };
    }

    private SymbolOrSelector ParseSymbolOrSelector(Symbol_or_selectorContext sosCtx, string[] usingNamespaces, Compilation compilation)
    {
        if (sosCtx is null)
        {
            throw new InvalidOperationException();
        }

        if (sosCtx.exception is not null)
        {
            throw new FormatException(sosCtx.exception.Message);
        }

        if (sosCtx.selector() is SelectorContext selector)
        {
            UtilityTypeSelector uts = ParseSelector(selector)
                ?? throw new FormatException("Failed to parse selector");

            return new(uts);
        }
        else if (sosCtx.symbol() is SymbolContext symbolCtx)
        {
            string symbolText = symbolCtx.GetText();
            INamedTypeSymbol symbol = compilation.GetTypesByName(symbolText, usingNamespaces).FirstOrDefault()
                ?? throw new TypeLoadException("Symbol not found");
            return new(symbol);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }
}