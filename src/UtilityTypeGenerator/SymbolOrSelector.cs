namespace UtilityTypeGenerator;

using System;
using Microsoft.CodeAnalysis;

public record SymbolOrSelector
{
    private SymbolOrSelector(INamedTypeSymbol? symbol, UtilityTypeSelector? selector)
    {
        Selector = selector;
        Symbol = symbol;
    }

    public SymbolOrSelector(UtilityTypeSelector? selector)
        : this(null, selector)
    {
    }

    public SymbolOrSelector(INamedTypeSymbol? symbol)
        : this(symbol, null)
    {
    }

    public UtilityTypeSelector? Selector { get; }
    public INamedTypeSymbol? Symbol { get; }

    public Accessibility Accessibility => Selector?.Accessibility ?? Symbol?.DeclaredAccessibility ?? throw new InvalidOperationException("SelectorOrSymbol has neither Selector nor Symbol.");

    public bool HasValue => Selector is not null || Symbol is not null;

    public PropertyRecord[] GetPropertyRecords(Compilation compilation)
    {
        return Selector?.GetPropertyRecords(compilation)
            ?? Symbol?.GetPropertyRecords()
            ?? throw new InvalidOperationException("SelectorOrSymbol has neither Selector nor Symbol.");
    }
}
