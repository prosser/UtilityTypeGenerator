namespace UtilityTypeGenerator.UnitTests;

internal struct TestStruct
{
    public int Foo { get; set; }
}

internal class TestGenericPoco<T>
{
    public T Value { get; set; } = default!;
}

internal class TestPoco
{
    public int NotNullInt { get; set; }
    public object NotNullObject { get; set; } = default!;
    public string NotNullString { get; set; } = default!;
    public TestStruct NotNullStruct { get; set; }
    public int? NullableInt { get; set; }
    public object? NullableObject { get; set; }
    public string? NullableString { get; set; }
    public TestStruct? NullableStruct { get; set; }
}

// must be the same as TestPoco
internal class TestPoco2
{
    public int NotNullInt { get; set; }
    public object NotNullObject { get; set; } = default!;
    public string NotNullString { get; set; } = default!;
    public TestStruct NotNullStruct { get; set; }
    public int? NullableInt { get; set; }
    public object? NullableObject { get; set; }
    public string? NullableString { get; set; }
    public TestStruct? NullableStruct { get; set; }
}

// must contain all not-null properties of TestPoco
internal class TestPocoOnlyNotNull
{
    public int NotNullInt { get; set; }
    public object NotNullObject { get; set; } = default!;
    public string NotNullString { get; set; } = default!;
    public TestStruct NotNullStruct { get; set; }
}

// must contain all nullable properties of TestPoco
internal class TestPocoOnlyNullable
{
    public int? NullableInt { get; set; }
    public object? NullableObject { get; set; }
    public string? NullableString { get; set; }
    public TestStruct? NullableStruct { get; set; }
}

internal class TestPocoViaInheritance : TestPocoOnlyNullable
{
    public int NotNullInt { get; set; }
    public object NotNullObject { get; set; } = default!;
    public string NotNullString { get; set; } = default!;
    public TestStruct NotNullStruct { get; set; }
}

internal class TestPocoWithDifferentProperties
{
    public int NotNullInt2 { get; set; }
    public object NotNullObject2 { get; set; } = default!;
    public string NotNullString2 { get; set; } = default!;
    public TestStruct NotNullStruct2 { get; set; }
    public int? NullableInt2 { get; set; }
    public object? NullableObject2 { get; set; }
    public string? NullableString2 { get; set; }
    public TestStruct? NullableStruct2 { get; set; }
}