namespace UtilityTypeGenerator.Selectors;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

/// <summary>
/// Selector that removes 'required' from all properties.
/// </summary>
/// <remarks>
/// Composition constructor.
/// </remarks>
/// <param name="selector">Selector or type symbol containing properties to strip 'required' from.</param>
public class OptionalSelector(SymbolOrSelector selector) : UtilityTypeSelector(selector)
{
    protected override IEnumerable<PropertyRecord> Transform(IEnumerable<PropertyRecord> properties, Compilation compilation)
    {
        return properties.Select(p => new PropertyRecord(p.Name, p.Type, p.Nullable, p.Readonly, false)).ToArray();
    }
}