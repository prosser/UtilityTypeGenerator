namespace UtilityTypeGenerator.Selectors;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

public class UnionSelector(Accessibility accessibility, IEnumerable<SymbolOrSelector> selectors)
    : UtilityTypeSelector(accessibility, compiler =>
    {
        PropertyRecordComparer comparer = new();
        return selectors.SelectMany(s => s.GetPropertyRecords(compiler))
            .Distinct(comparer)
            .ToArray();
    })
{
    protected override IEnumerable<PropertyRecord> Transform(IEnumerable<PropertyRecord> properties, Compilation compilation)
    {
        IGrouping<string, PropertyRecord>[] conflicts = properties
            .GroupBy(p => p.Name)
            .Where(g => g.Count() > 1 && g.GroupBy(p => p.Type.ToDisplayString()).Count() > 1)
            .ToArray();

        string[] conflictMessages = conflicts.Select(g => string.Join(", ", g.Select(p => $"{p.Type.ToDisplayString()} {p.Name}")))
            .ToArray();

        return conflicts.Length == 0
            ? properties
            : throw new InvalidOperationException(
                $"Conflicting property names with different types: {string.Join("; ", conflictMessages)}.");
    }
}