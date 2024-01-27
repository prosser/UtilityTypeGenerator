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
public class PickSelector(Accessibility accessibility, SymbolOrSelector selector, string[] propertyNames)
    : UtilityTypeSelector(accessibility, selector)
{
    private readonly HashSet<string> propertyNames = [.. propertyNames];

    protected override IEnumerable<PropertyRecord> Transform(IEnumerable<PropertyRecord> properties, Compilation compilation)
    {
        string[] missingPropertyNames = propertyNames.Except(properties.Select(x => x.Name)).ToArray();

        return missingPropertyNames.Length > 0
            ? throw new ArgumentException(string.Join(", ", missingPropertyNames))
            : properties.Where(x => propertyNames.Contains(x.Name));
    }
}