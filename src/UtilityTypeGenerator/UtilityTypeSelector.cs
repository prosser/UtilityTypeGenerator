namespace UtilityTypeGenerator;

using Microsoft.CodeAnalysis;

public abstract class UtilityTypeSelector(Accessibility accessibility, Func<Compilation, PropertyRecord[]> getProperties)
{
    private readonly Func<Compilation, PropertyRecord[]> getProperties = getProperties;

    protected UtilityTypeSelector(Accessibility accessibility, SymbolOrSelector selectorOrSymbol)
        : this(accessibility, selectorOrSymbol.HasValue ? selectorOrSymbol.GetPropertyRecords : throw new ArgumentOutOfRangeException(nameof(selectorOrSymbol)))
    {
    }

    protected UtilityTypeSelector(Accessibility accessibility, IEnumerable<SymbolOrSelector> selectorsOrSymbols, Func<IEnumerable<SymbolOrSelector>, Compilation, IEnumerable<PropertyRecord>> reducer)
        : this(accessibility, compilation => [.. reducer(selectorsOrSymbols, compilation)])
    {
    }

    public Accessibility Accessibility { get; } = accessibility;

    /// <summary>
    /// Gets the properties that are selected for inclusion in the generated type.
    /// </summary>
    public PropertyRecord[] GetPropertyRecords(Compilation compilation)
    {
        return [.. Transform(getProperties(compilation), compilation)];
    }

    protected abstract IEnumerable<PropertyRecord> Transform(IEnumerable<PropertyRecord> properties, Compilation compilation);
}