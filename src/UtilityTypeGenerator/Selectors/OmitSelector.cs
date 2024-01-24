namespace UtilityTypeGenerator.Selectors;

using Microsoft.CodeAnalysis;

/// <summary>
/// Selector that includes all properties except the ones specified.
/// </summary>
/// <remarks>
/// Composition constructor.
/// </remarks>
/// <param name="selector">Selector that contains properties from which the omission will apply.</param>
/// <param name="propertyNames">Property names to omit.</param>
public class OmitSelector(SelectorOrSymbol selector, string[] propertyNames) : UtilityTypeSelector(selector)
{
    private readonly HashSet<string> propertyNames = [.. propertyNames];

    protected override IEnumerable<PropertyRecord> Transform(IEnumerable<PropertyRecord> properties, Compilation compilation)
    {
        return properties.Where(x => !propertyNames.Contains(x.Name));
    }
}