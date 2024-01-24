namespace UtilityTypeGenerator.Selectors;

using System.Linq;
using Microsoft.CodeAnalysis;

/// <summary>
/// A selector that picks properties from a type.
/// </summary>
/// <remarks>
/// Composition constructor.
/// </remarks>
/// <param name="selector">Selector containing properties to pick from.</param>
/// <param name="propertyNames">Property names to pick.</param>
public class PickSelector(SelectorOrSymbol selector, string[] propertyNames) : UtilityTypeSelector(selector)
{
    private readonly HashSet<string> propertyNames = [.. propertyNames];

    protected override IEnumerable<PropertyRecord> Transform(IEnumerable<PropertyRecord> properties, Compilation compilation)
    {
        return properties.Where(x => propertyNames.Contains(x.Name));
    }
}