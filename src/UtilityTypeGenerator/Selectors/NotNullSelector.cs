namespace UtilityTypeGenerator.Selectors;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

/// <summary>
/// Selector that transforms all properties to non-nullable.
/// </summary>
/// <remarks>
/// Composition constructor.
/// </remarks>
/// <param name="nestedSelector">Selector containing properties to make non-nullable.</param>
public class NotNullSelector(SelectorOrSymbol nestedSelector) : UtilityTypeSelector(nestedSelector)
{
    protected override IEnumerable<PropertyRecord> Transform(IEnumerable<PropertyRecord> properties, Compilation compilation)
    {
        return properties.Select(p => p.MakeNotNull());
    }
}