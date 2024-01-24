namespace UtilityTypeGenerator;

using System;
using Microsoft.CodeAnalysis;

public record SelectorOrSymbol
{
    private SelectorOrSymbol(UtilityTypeSelector? selector, INamedTypeSymbol? symbol)
    {
        Selector = selector;
        Symbol = symbol;
    }

    public SelectorOrSymbol(UtilityTypeSelector? selector)
        : this(selector, null)
    {
    }

    public SelectorOrSymbol(INamedTypeSymbol? symbol)
        : this(null, symbol)
    {
    }

    public UtilityTypeSelector? Selector { get; }
    public INamedTypeSymbol? Symbol { get; }

    public bool HasValue => Selector is not null || Symbol is not null;

    public PropertyRecord[] GetPropertyRecords(Compilation compilation)
    {
        return Selector?.GetPropertyRecords(compilation)
            ?? Symbol?.GetPropertyRecords()
            ?? throw new InvalidOperationException("SelectorOrSymbol has neither Selector nor Symbol.");
    }
}
