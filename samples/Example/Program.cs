namespace Example;

using System.Linq;
using System.Reflection;
using UtilityTypeGenerator;

public class Person
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public DateTime? BirthDate { get; set; }
}

public class Position
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string JobTitle { get; set; } = "";
}

[UtilityType("Import<Position>")]
public partial class Foo
{
    public string FooBar { get; set; } = "";
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

public class Program
{
    public static void Main()
    {
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