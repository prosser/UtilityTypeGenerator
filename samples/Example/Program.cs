namespace Example
{
    using System.Linq;
    using System.Reflection;
    using Public;

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
}

namespace Internal
{
    using UtilityTypeGenerator;
    using Public;

    [UtilityType("Required<Person>")]
    internal partial class AllPropertiesAreRequired;

    internal class GenericValue<T>
    {
        public T? Value { get; set; }
    }

    [UtilityType("Required<Union<Person, Position>>")]
    internal partial class PersonAndPositionEverythingRequired;

    [UtilityType("Required<Person>")]
    internal partial class PersonEverythingRequired;

    [UtilityType("Nullable<Union<Person, Position>>")]
    public partial record ChangeToARecordType;

    [UtilityType("Import<Person>")]
    internal partial interface IPerson2;
}

namespace Public
{
    using UtilityTypeGenerator;
    using Internal;

    [UtilityType("Intersection<Person, Position>")]
    public partial class AlsoIdAndName;

    [UtilityType("Pick<Person, Id | Name>")]
    public partial class IdAndName;

    [UtilityType("Pick<Person, Name>")]
    public partial class OnlyName;

    [UtilityType("Import<Person>")]
    public partial interface IPerson;

    public class Person
    {
        public DateTimeOffset? BirthDate { get; set; }

        /// <summary>
        /// The unique identifier for the person.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The name for the person.
        /// </summary>
        public string? Name { get; set; }

        internal GenericValue<Position?> GenericValue { get; set; } = new();
    }

    [UtilityType("Union<Person, Position>")]
    public partial class PersonAndPosition;

    [UtilityType("Import<Person>")]
    public partial class PersonWithMessage
    {
        public string Message { get; set; } = "";
    }

    public class Position
    {
        public Guid Id { get; set; }
        public string JobTitle { get; set; } = "";
        public string? Name { get; set; }
    }

    [UtilityType("Nullable<Position>")]
    public partial class PositionEverythingNullable;

    [UtilityType("Omit<Person, Name>")]
    public partial class WithoutName;
}