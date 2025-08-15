namespace UtilityTypeGenerator.Selectors;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

/// <summary>
/// Selector that transforms all properties to nullable.
/// </summary>
/// <remarks>
/// Composition constructor.
/// </remarks>
/// <param name="selector">Selector or type symbol containing properties to make nullable.</param>
public class NullableSelector(Accessibility accessibility, SymbolOrSelector selector)
    : UtilityTypeSelector(accessibility, selector)
{
    protected override IEnumerable<PropertyRecord> Transform(IEnumerable<PropertyRecord> properties, Compilation compilation)
    {
        return [.. properties.Select(p => p.MakeNullable(compilation))];
    }
}