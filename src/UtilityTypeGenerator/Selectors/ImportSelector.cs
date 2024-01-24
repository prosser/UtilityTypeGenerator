namespace UtilityTypeGenerator.Selectors;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

/// <summary>
/// Selector that imports properties from another type.
/// </summary>
/// <remarks>
/// Composition constructor.
/// </remarks>
/// <param name="selector">Selector or type symbol containing properties to import.</param>
public class ImportSelector(SymbolOrSelector selector) : UtilityTypeSelector(selector)
{
    protected override IEnumerable<PropertyRecord> Transform(IEnumerable<PropertyRecord> properties, Compilation compilation)
    {
        return properties;
    }
}