namespace UtilityTypeGenerator.Selectors;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

public class UnionSelector(IEnumerable<SelectorOrSymbol> selectors) : UtilityTypeSelector(compiler =>
    {
        PropertyRecordComparer comparer = new();
        return selectors.SelectMany(s => s.GetPropertyRecords(compiler))
            .Distinct(comparer)
            .ToArray();
    })
{
    protected override IEnumerable<PropertyRecord> Transform(IEnumerable<PropertyRecord> properties, Compilation compilation)
    {
        string[] conflicts = properties
            .GroupBy(p => p.Name)
            .Where(g => g.Count() > 1)
            .Select(x => x.Key)
            .ToArray();
        return conflicts.Length == 0
            ? properties
            : throw new InvalidOperationException(
                $"Conflicting property names with different types: {string.Join(", ", conflicts)}.");
    }
}