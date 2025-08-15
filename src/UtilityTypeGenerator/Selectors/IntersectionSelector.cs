namespace UtilityTypeGenerator.Selectors;

using System;
using Microsoft.CodeAnalysis;

public class IntersectionSelector(Accessibility accessibility, IEnumerable<SymbolOrSelector> selectors)
    : UtilityTypeSelector(accessibility, compiler =>
    {
        PropertyRecordComparer comparer = new();
        return [.. selectors
            .Select(s => s.GetPropertyRecords(compiler))
            .Cast<IEnumerable<PropertyRecord>>()
            .Aggregate((a, b) => a.Intersect(b, comparer))
            .Distinct(comparer)];
    })
{
    protected override IEnumerable<PropertyRecord> Transform(IEnumerable<PropertyRecord> properties, Compilation compilation)
    {
        IGrouping<string, PropertyRecord>[] conflicts = [.. properties
            .GroupBy(p => p.Name)
            .Where(g => g.Count() > 1 && g.GroupBy(p => p.Type.ToDisplayString()).Count() > 1)];

        string[] conflictMessages = [.. conflicts.Select(g => string.Join(", ", g.Select(p => $"{p.Type.ToDisplayString()} {p.Name}")))];

        return conflicts.Length == 0
            ? properties
            : throw new InvalidOperationException(
                $"Conflicting property names with different types: {string.Join("; ", conflictMessages)}.");
    }
}
