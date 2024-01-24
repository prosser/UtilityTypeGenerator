namespace UtilityTypeGenerator;

using Microsoft.CodeAnalysis;

public abstract class UtilityTypeSelector(Func<Compilation, PropertyRecord[]> getProperties)
{
    private readonly Func<Compilation, PropertyRecord[]> getProperties = getProperties;
    protected UtilityTypeSelector(SymbolOrSelector selectorOrSymbol)
        : this(selectorOrSymbol.HasValue ? selectorOrSymbol.GetPropertyRecords : throw new ArgumentOutOfRangeException(nameof(selectorOrSymbol)))
    {
    }

    protected UtilityTypeSelector(IEnumerable<SymbolOrSelector> selectorsOrSymbols, Func<IEnumerable<SymbolOrSelector>, Compilation, IEnumerable<PropertyRecord>> reducer)
        : this(compilation => reducer(selectorsOrSymbols, compilation).ToArray())
    {
    }

    /// <summary>
    /// Gets the properties that are selected for inclusion in the generated type.
    /// </summary>
    public PropertyRecord[] GetPropertyRecords(Compilation compilation)
    {
        return Transform(getProperties(compilation), compilation).ToArray();
    }

    protected abstract IEnumerable<PropertyRecord> Transform(IEnumerable<PropertyRecord> properties, Compilation compilation);
}
