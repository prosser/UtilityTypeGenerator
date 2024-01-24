namespace UtilityTypeGenerator;

using System;
using Microsoft.CodeAnalysis;

public sealed record PropertyRecord(string Name, ITypeSymbol Type, bool Nullable, bool Readonly, bool Required) : IEquatable<PropertyRecord>
{
    //public static implicit operator PropertyRecord((string, ITypeSymbol, bool, bool, ) tuple)
    //{
    //    return new PropertyRecord(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
    //}

    public PropertyRecord MakeNotNull()
    {
        return !Nullable ? this : new(Name, Type.MakeNotNull(), false, Readonly, Required);
    }

    public PropertyRecord MakeNullable(Compilation compilation)
    {
        return Nullable ? this : new(Name, Type.MakeNullable(compilation), true, Readonly, Required);
    }

    public bool Equals(PropertyRecord? other)
    {
        return other is not null &&
            Name == other.Name &&
            Nullable == other.Nullable &&
            Readonly == other.Readonly &&
            Required == other.Required &&
            (SymbolEqualityComparer.Default.Equals(Type, other.Type) ||
            Type.IsNullableEquals(other.Type));
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, SymbolEqualityComparer.Default.GetHashCode(Type), Nullable);
    }
}
