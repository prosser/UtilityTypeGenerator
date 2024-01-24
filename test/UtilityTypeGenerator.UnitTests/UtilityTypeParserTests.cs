namespace UtilityTypeGenerator.UnitTests;

using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public class UtilityTypeParserTests
{
    private static readonly Lazy<CSharpCompilation> Compilation = new(() => CSharpCompilation.Create(
            "test",
            references:
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(UtilityTypeParserTests).Assembly.Location),
            ]), LazyThreadSafetyMode.ExecutionAndPublication);

    private static readonly PropertyRecordComparer PropertyComparer = new();
    public record ExpectedSelector(Type SelectorType, PropertyRecord[] Properties);
    private static readonly Dictionary<string, INamedTypeSymbol> Symbols = LoadSymbols(Compilation.Value);

    public static TheoryData<string, string, PropertyRecord[]?> GetIntersectionTestData()
    {
        TheoryData<string, string, PropertyRecord[]?> data = [];

        PropertyRecord[] notNullProperties = typeof(TestPocoOnlyNotNull).GetProperties()
            .Select(p => new PropertyRecord(p.Name, Symbols[p.Name], false, p.GetAccessors(true).Length < 2, false)).ToArray();

        PropertyRecord[] nullableProperties = typeof(TestPocoOnlyNullable).GetProperties()
            .Select(p => new PropertyRecord(p.Name, Symbols[p.Name], true, p.GetAccessors(true).Length < 2, false)).ToArray();

        // can do intersect with either "Intersect" or "Intersection" keywords
        data.Add("intersect1", "Intersect<TestPocoOnlyNotNull, TestPoco>", notNullProperties);
        data.Add("intersect2", "Intersection<TestPoco, TestPocoOnlyNullable>", nullableProperties);

        return data;
    }

    public static TheoryData<string, string, PropertyRecord[]?> GetOmitTestData()
    {
        TheoryData<string, string, PropertyRecord[]?> data = [];

        string[] omitAllBut1 = typeof(TestPoco).GetProperties().Where(x => x.Name != "NotNullInt").Select(x => x.Name).ToArray();
        data.Add("omit1", $"Omit<TestPoco, {string.Join('|', omitAllBut1)}>",
            [
                new("NotNullInt", Symbols["NotNullInt"], false, false, false),
            ]);

        string[] omitAllBut2 = typeof(TestPoco).GetProperties().Where(x => x.Name is not "NotNullInt" and not "NotNullObject").Select(x => x.Name).ToArray();
        data.Add("omit2", $"Omit<Omit<TestPoco, {string.Join('|', omitAllBut1)}>, NotNullObject>",
            [new("NotNullInt", Symbols["NotNullInt"], false, false, false)]);

        return data;
    }

    public static TheoryData<string, string, PropertyRecord[]?> GetPickTestData()
    {
        TheoryData<string, string, PropertyRecord[]?> data = [];

        data.Add("pick1", "Pick<TestPoco, NotNullInt | NotNullObject>",
            [
                new("NotNullInt", Symbols["NotNullInt"], false, false, false),
                new("NotNullObject", Symbols["NotNullObject"], false, false, false)
            ]);

        data.Add("pick2", "Pick<Pick<TestPoco, NotNullInt | NotNullObject | NullableObject>, NotNullInt>",
            [new("NotNullInt", Symbols["NotNullInt"], false, false, false)]);

        data.Add("pick2-no-whitespace", "Pick<Pick<TestPoco,NotNullInt|NotNullObject|NullableObject>,NotNullInt>",
            [new("NotNullInt", Symbols["NotNullInt"], false, false, false)]);

        //data.Add("pick2-leading-comment", "/* leading comment */ Pick<Pick<TestPoco, NotNullInt | NotNullObject | NullableObject>, NotNullInt>",
        //    [new("NotNullInt", Symbols["NotNullInt"], false, false, false)]);

        //data.Add("pick2-trailing-comment", "Pick<Pick<TestPoco, NotNullInt | NotNullObject | NullableObject>, NotNullInt> // trailing comment",
        //    [new("NotNullInt", Symbols["NotNullInt"], false, false, false)]);

        //data.Add("pick2-inline-comment", "Pick<Pick<TestPoco, /* comment okay? */ NotNullInt | NotNullObject | NullableObject>, NotNullInt>",
        //    [new("NotNullInt", Symbols["NotNullInt"], false, false, false)]);

        return data;
    }

    public static TheoryData<string, string, PropertyRecord[]?> GetRequiredTestData()
    {
        TheoryData<string, string, PropertyRecord[]?> data = [];

        // depends on the fact that TestPoco is the union of TestPocoOnlyNotNull and TestPocoOnlyNullable already,
        PropertyRecord[] testPocoProperties = typeof(TestPoco).GetProperties()
            .Select(p => GetTestRecord(p, pr => pr with { Required = true })).ToArray();

        data.Add("Required", "Required<TestPoco>", testPocoProperties);

        return data;
    }

    public static TheoryData<string, string, PropertyRecord[]?> GetOptionalTestData()
    {
        TheoryData<string, string, PropertyRecord[]?> data = [];

        // depends on the fact that TestPoco is the union of TestPocoOnlyNotNull and TestPocoOnlyNullable already,
        PropertyRecord[] testPocoProperties = typeof(TestPoco).GetProperties()
            .Select(p => GetTestRecord(p, pr => pr with { Required = false })).ToArray();

        data.Add("Optional", "Optional<TestPoco>", testPocoProperties);

        return data;
    }

    public static TheoryData<string, string, PropertyRecord[]?> GetUnionTestData()
    {
        TheoryData<string, string, PropertyRecord[]?> data = [];

        // depends on the fact that TestPoco is the union of TestPocoOnlyNotNull and TestPocoOnlyNullable already,
        PropertyRecord[] testPocoProperties = typeof(TestPoco).GetProperties()
            .Select(p => GetTestRecord(p)).ToArray();

        data.Add("union1", "Union<TestPocoOnlyNotNull, TestPocoOnlyNullable>", testPocoProperties);

        // depends on the fact that TestPoco and TestPoco2 are identical
        data.Add("union2", "Union<TestPoco, TestPoco2>", testPocoProperties);

        return data;
    }

    [Theory]
    [MemberData(nameof(GetPickTestData))]
    [MemberData(nameof(GetOmitTestData))]
    [MemberData(nameof(GetUnionTestData))]
    [MemberData(nameof(GetIntersectionTestData))]
    [MemberData(nameof(GetRequiredTestData))]
    public void ParserCanParseGrammarSyntax(string name, string input, PropertyRecord[]? expected)
    {
        // Arrange
        UtilityTypeSelector? selector = UtilityTypeHelper.Parse(["UtilityTypeGenerator.UnitTests"], Compilation.Value, input);

        if (expected is null)
        {
            selector.Should().BeNull($"{name} should have a null selector");
            return;
        }

        selector.Should().NotBeNull($"{name} should have a non-null selector");
        PropertyRecord[] properties = selector!.GetPropertyRecords(Compilation.Value);

        properties.Should().BeEquivalentTo(expected, o => o.Using(PropertyComparer), $"{name} properties should match");
    }

    private static PropertyRecord GetTestRecord(PropertyInfo p, Func<PropertyRecord, PropertyRecord>? configure = null)
    {
        PropertyRecord pr = new(p.Name, Symbols[p.Name], p.Name.StartsWith("Nullable"), p.GetAccessors(true).Length < 2, false);

        return configure?.Invoke(pr) ?? pr;
    }

    private static Dictionary<string, INamedTypeSymbol> LoadSymbols(CSharpCompilation compilation)
    {
        // get the test type symbols
        var symbols = typeof(TestPoco).Assembly.GetTypes()
            .Where(x => x.Name.StartsWith("Test") && x.IsClass)
            .Select(x => (x.FullName!, compilation.GetTypesByName(x.FullName).FirstOrDefault()))
            .Where(x => x.Item2 is not null)
            .ToDictionary(a => a.Item1, a => a.Item2!);

        INamedTypeSymbol testStruct = compilation.GetTypesByName("UtilityTypeGenerator.UnitTests.TestStruct").Single();

        symbols.Add(nameof(TestPoco.NotNullInt), compilation.GetSpecialType(SpecialType.System_Int32).MakeNotNull());
        symbols.Add(nameof(TestPoco.NotNullObject), compilation.GetSpecialType(SpecialType.System_Object).MakeNotNull());
        symbols.Add(nameof(TestPoco.NotNullString), compilation.GetSpecialType(SpecialType.System_String).MakeNotNull());
        symbols.Add(nameof(TestPoco.NotNullStruct), testStruct.MakeNotNull());
        symbols.Add(nameof(TestPoco.NullableInt), compilation.GetSpecialType(SpecialType.System_Int32).MakeNullable(compilation));
        symbols.Add(nameof(TestPoco.NullableObject), compilation.GetSpecialType(SpecialType.System_Object).MakeNullable(compilation));
        symbols.Add(nameof(TestPoco.NullableString), compilation.GetSpecialType(SpecialType.System_String).MakeNullable(compilation));
        symbols.Add(nameof(TestPoco.NullableStruct), testStruct.MakeNullable(compilation));

        return symbols;
    }
}