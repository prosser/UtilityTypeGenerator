namespace Example;

using System.Linq;
using System.Reflection;
using UtilityTypeGenerator;

public class Person
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset? BirthDate { get; set; }
}

public class Position
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string JobTitle { get; set; } = "";
}

[UtilityType("Import<Person>")]
public partial class PersonWithMessage
{
    public string Message { get; set; } = "";
}

[UtilityType("Required<Person>")]
public partial class AllPropertiesAreRequired;

[UtilityType("Pick<Person, Name>")]
public partial class OnlyName;

[UtilityType("Pick<Person, Id | Name>")]
public partial class IdAndName;

[UtilityType("Omit<Person, Name>")]
public partial class WithoutName;

[UtilityType("Intersection<Person, Position>")]
public partial class AlsoIdAndName;

[UtilityType("Union<Person, Position>")]
public partial class PersonAndPosition;

[UtilityType("Required<Person>")]
public partial class PersonEverythingRequired;

[UtilityType("Required<Union<Person, Position>>")]
public partial class PersonAndPositionEverythingRequired;

[UtilityType("Nullable<Position>")]
public partial class PositionEverythingNullable;

[UtilityType("Nullable<Union<Person, Position>>")]
public partial record ChangeToARecordType;

public class Program
{
    public static void Main()
    {
        PersonWithMessage foo = new()
        {
            Message = "Hello, World!",
            Id = Guid.Empty,
            Name = "Peter Rosser",
            BirthDate = new DateTimeOffset(1975, 11, 7, 15, 32, 0, TimeSpan.FromHours(-9)),
        };

        foreach (Type type in typeof(Program).Assembly.GetTypes().OrderBy(x => x.Name))
        {
            Console.WriteLine($"{type.Name} has these properties:");
            foreach (PropertyInfo property in type.GetProperties())
            {
                Console.WriteLine($"{property.Name} = {property.PropertyType.FullName}");
            }

            Console.WriteLine();
        }
    }
}