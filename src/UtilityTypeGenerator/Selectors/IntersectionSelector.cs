namespace UtilityTypeGenerator.Selectors;

using System;
using Microsoft.CodeAnalysis;

public class IntersectionSelector(IEnumerable<SelectorOrSymbol> selectors) : UtilityTypeSelector(compiler =>
    {
        PropertyRecordComparer comparer = new();
        return selectors
            .Select(s => s.GetPropertyRecords(compiler))
            .Cast<IEnumerable<PropertyRecord>>()
            .Aggregate((a, b) => a.Intersect(b, comparer))
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
