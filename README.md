# UtilityTypeGenerator

Generates TypeScript-like utility types for C#.

Utility types are generated types based on one or more input types. Slap the `[UtilityType(arg)]` attribute on a
`partial` type and the generator will generate a partial type with the same name and type (e.g., class, record, struct)
as the input type, but with the specified utility type applied.

Utility types can be composed to create new utility types, e.g. `Required<Union<T1, T2>>`.

> **Important Note:** This only generates auto-properties, no matter whether the input type's properties are auto-properties or not.
> This can be handy in and of itself, but computed properties are out of scope for this project.

## Usage

1. Add the **UtilityTypeGenerator** NuGet package to your project (pick your flavor):
    - .csproj: `<PackageReference Include="UtilityTypeGenerator" Version="0.0.2" />`
    - dotnet CLI: `dotnet add package UtilityTypeGenerator`
1. Add the `[UtilityType(arg)]` attribute to a `partial` type.

## Supported utility types

- `Pick<T, Property1 | Property2 | ...>`
- `Omit<T, Property1 | Property2 | ...>`
- `Required<T>`
- `Optional<T>`
- `Nullable<T>`*
- `Import<T>`
- `Union<T1, T2, ...>`
- `Intersection<T1, T2, ...>`

> \* `Optional<T>` behaves differently than it does in TypeScript! [See below for details](#Optional).

### Import

Imports all of the properties from `T` (a type or another utility type).

Syntax: `Import<T>`

#### Example

```csharp
public class Person
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset? BirthDate { get; set; }
}

[UtilityType("Import<Person>")]
public partial class Foo
{
    public required string SomeOtherProperty { get; }
}
```

```csharp
// generates:
public partial class Foo
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset? BirthDate { get; set; }
}

// since this is a partial class, SomeOtherProperty is also defined.
```

### Pick

Creates a type with only the specified properties from `T` (a type or another utility type).

Property names are separated by `|` (pipe) characters, and must exist on the input type.

Syntax: `Pick<T, Property1 | Property2 | ...>`

#### Example

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

Creates a type with all properties from `T` (a type or another utility type) except the specified properties.

Property names are separated by `|` (pipe) characters, and must exist on the input type.

Syntax: `Omit<T, Property1 | Property2 | ...>`

#### Example

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

Creates a type with all properties from `T` (a type or another utility type) transformed to non-nullable.

Syntax: `Required<T>`

#### Example

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

Creates a type with all properties from `T` (a type or another utility type) stripped of the `required` keyword.

> IMPORTANT: This is different from TypeScript, where `Optional<T>` allows `undefined` values for the properties.

> TIP: This should be combined with `Nullable<T>` to avoid NullReferenceExceptions for reference types.

Syntax: `Required<T>`

#### Example

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

Creates a type with all properties from `T` (a type or another utility type) transformed to nullable.

Syntax: `Nullable<T>`

#### Example

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

Creates a type with the union of properties from `T1` and `T2`, etc. (types or other utility types). Duplicate properties are okay,
but the type of the property must be the same in both types.

Syntax: `Union<T1, T2, ...>`

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

Creates a type with the intersection of properties from `T1` and `T2`, etc. (types or other utility types). Duplicate properties are okay,
but the type of the property must be the same in both types.

Syntax: `Intersection<T1, T2, ...>`

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

## A note on error handling and implementation choices

Don't expect much. Garbage in, garbage out, and all that. That said, the code generator prefers to 
emit nothing if you have a syntax error. If you're not getting the type generated, here are the most
likely causes:

- Misspellings and typos (that hardly needs to be said, right?)
- Property name in `Pick` or `Omit` that doesn't actually exist in the type.
- make sure your `<>` chars are balanced!
- `|` is the property name delimiter, not `,`. (for `Pick` and `Omit`)
- `Union` and `Intersection` require at least 2 type arguments.

If this gets at all popular, I'll add some compiler warnings, syntax highlighting & error checking (red-squiggles!), etc.

I chose to use a string argument instead of more C#-like syntax to allow for a more compact syntax that is identical in
nearly every case to the TypeScript syntax. Under the covers, the generator uses ANTLR with a simple grammar to do the parsing,
and extending it to support more selectors is fairly trivial.

If there's demand for a more verbose syntax, I'll consider adding it (or you can submit a PR).
