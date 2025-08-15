# UtilityTypeGenerator

Generates TypeScript-like utility types for C#.

Utility types are generated types based on one or more input types. Slap the `[UtilityType(selector)]` attribute on a
`partial` type and the generator will generate a partial type with the same name and type (e.g., class, record, struct)
as the type with the attribute (yes, that can be different from the input type(s)!), but with the specified selector(s) applied.

For more information about utility types and how to use them, check out [the TypeScript docs](https://www.typescriptlang.org/docs/handbook/utility-types.html).

> **Important Note:** This only generates auto-properties, no matter whether the input type's properties are auto-properties or not.
> This can be handy in and of itself, but computed properties are out of scope for this project.

> **Another important note:** This is a source generator, so it only works w/ .NET 5.0+. However, I'm opinionated about using the latest stable C# SDK, so YMMV if you are running something ancient (like C# 7.3). You should really be setting `<LangVersion>latest</LangVersion>` in your projects (yes, that works with older TargetFrameworks).

## Usage

1. Add the **UtilityTypeGenerator** NuGet package to your project:
    - `<PackageReference Include="UtilityTypeGenerator" Version="1.0.0" PrivateAssets="all" IncludeAssets="build; analyzers" />
`
1. Add the `[UtilityType("selector")]` attribute to a `partial` type, replacing `"selector"` with the selector(s) of your choice.

## Notes on generated types

- The generated type will be a `partial` type with the same name, type, and accessibility as the type with the `[UtilityType]` attribute.
- Each property will have the same accessibility as the property in the input type.
- Comments (leading trivia) from the first matching property in the input type will be copied to the generated property.
- Initializer statements for properties will be stripped. If the property type is not nullable, the `= default!;` initializer will be added.

If you need to customize the generated type, you can simply provide whatever you need in the partial type. Make sure to exclude any properties that you don't want to be generated if they would conflict!

## Supported selectors

A selector is a string that specifies a verb (e.g., `Pick`), one or more types or nested selectors, and (for some verbs) property names.

| Verb | Syntax | Description |
| ---- | ------ | ----------- |
| `Import` | `Import<T>` | Imports all of the properties from `T` (a type or selector). |
| `Intersection` | `Intersection<T1, T2 [, T3] [...]>` or `Intersect<T1, T2 [, T3] [...]>` | Creates a type with the intersection of properties from `T1` and `T2`, etc. (types or selectors). Duplicate properties are okay, but the type of the property must be the same in both types. |
| `NotNull` | `NotNull<T>` | Creates a type with all properties from `T` (a type or selector) transformed to non-nullable.|
| `Nullable` | `Nullable<T>` | Creates a type with all properties from `T` (a type or selector) transformed to nullable.|
| `Omit` | `Omit<T, Property1 [\| Property2] [...]>` or `Omit<T, Property1 [, Property2] [...]>` | Creates a type with all properties from `T` (a type or selector) except the specified properties. |
| `Optional`* | `Optional<T>` | Creates a type with all properties from `T` (a type or selector) stripped of the `required` keyword. <br>\* `Optional<T>` behaves differently than it does in TypeScript! [See below for details](#Optional). |
| `Pick` | `Pick<T, Property1 [\| Property2] [...]>` or `Pick<T, Property1 [, Property2] [...]>` | Creates a type with only the specified properties from `T` (a type or selector). |
| `Required` | `Required<T>` | Creates a type with all properties from `T` (a type or selector) marked as `required`.<br>Requires C# 11+ (or PolySharp!) |
| `Union` | `Union<T1, T2 [, T3] [...]>` | Creates a type with the union of properties from `T1` and `T2`, etc. (types or selectors). Duplicate properties are okay, but the type of the property must be the same in both types. At least 2 types must be present in the selector. |

## Examples

### Import

```csharp
namespace MyNamespace;

using MyNamespace.InternalData;

public class Person
{
    /// <summary>The unique identifier for the person.</summary>
    public Guid Id { get; set; }

    /// <summary>The name of the person.</summary>
    public string? Name { get; set; }

    /// <summary>The date of birth of the person.</summary>
    public DateTimeOffset? BirthDate { get; set; }

    /// <summary>Internal data object for that person.</summary>
    internal PersonData Data { get; set; }
}

[UtilityType("Import<Person>")]
internal partial class Foo
{
    public required string SomeOtherProperty { get; }
}
```

```csharp
// generates:
internal partial class Foo
{
    /// <summary>The unique identifier for the person.</summary>
    public Guid Id { get; set; }
    /// <summary>The name of the person.</summary>
    public string? Name { get; set; }
    /// <summary>The date of birth of the person.</summary>
    public DateTimeOffset? BirthDate { get; set; }
    /// <summary>Internal data object for that person.</summary>
    internal global::MyNamespace.InternalData.PersonData Data { get; set; }
}

// since this is a partial class, SomeOtherProperty is also defined (but not in the generated source).
```

### Pick

```csharp
public class Person
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset? BirthDate { get; set; }
}

[UtilityType("Pick<Person, Name>")]
public partial class OnlyName;
```

```csharp
// generates:
public partial class OnlyName
{
    public string? Name { get; set; }
}
```

### Omit

```csharp
public class Person
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset? BirthDate { get; set; }
}

[UtilityType("Omit<Person, Name>")]
public partial class OmitName;
```

```csharp
// generates:
public partial class OmitName
{
    public Guid Id { get; set; }
    public DateTimeOffset? BirthDate { get; set; }
}
```


### Required

```csharp
public class Person
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset? BirthDate { get; set; }
}

[UtilityType("Required<Person>")]
public partial class PersonRequired;
```

```csharp
// generates:
public partial class PersonRequired
{
    public Guid Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public DateTimeOffset BirthDate { get; set; } = default!;
}
```

### Optional

> IMPORTANT: This is different from TypeScript, where `Optional<T>` allows `undefined` values for the properties.

> TIP: This should be combined with `Nullable<T>` to avoid NullReferenceExceptions for any reference type properties.
> Composition with `Pick<T>` and `Omit<T>` can also be helpful.

```csharp
public class Person
{
    public required Guid Id { get; }
    public required string? Name { get; }
    public DateTimeOffset? BirthDate { get; set; }
}

[UtilityType("Optional<Person>")]
public partial class PersonOptional;
```

```csharp
// generates:
public partial class PersonOptional
{
    public Guid Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public DateTimeOffset? BirthDate { get; set; }
}
```

### Nullable

```csharp
public class Person
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public DateTimeOffset BirthDate { get; set; } = DateTimeOffset.MinValue;
}

[UtilityType("Nullable<Person>")]
public partial class PersonWithNullableProperties;
```

```csharp
// generates (note the default values are stripped!):
public partial class PersonWithNullableProperties
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset? BirthDate { get; set; }
}
```

### Union

> TIP: If you are trying to Union just one type, use `Import<T>` instead.

Syntax: `Union<T1, T2 [, T3] [...]>`

#### Example

```csharp
public class Person
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset? BirthDate { get; set; }
}

public class User
{
    public required Guid Id { get; set; }
    public required string? UserName { get; set; }
}

[UtilityType("Union<Person, User>")]
public partial class PersonAndUser;
```

```csharp
// generates:
public partial class PersonAndUser
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset? BirthDate { get; set; }
    public required string? UserName { get; set; }
}
```

### Intersection

```csharp
public class Person
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset? BirthDate { get; set; }
}

public class User
{
    public required Guid Id { get; set; }
    public required string? UserName { get; set; }
}

[UtilityType("Intersection<Person, User>")]
public partial class PersonAndUser;
```

```csharp
// generates:
public partial class PersonAndUser
{
    public Guid Id { get; set; }
}
```

## A note on implementation choices

If this gets at all popular, I'll add more compiler messages, syntax highlighting & error checking (red-squiggles!), etc.

I chose to use a string argument instead of more C#-like syntax to allow for a more compact syntax that is identical in
nearly every case to the TypeScript syntax. Under the covers, the generator uses ANTLR with a simple grammar to do the parsing,
and extending it to support more selectors is fairly trivial.

If there's demand for a more verbose syntax, I'll consider adding it (or you can submit a PR).
