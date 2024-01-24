namespace UtilityTypeGenerator;

public class PropertyRecordComparer : IEqualityComparer<PropertyRecord>
{
    public static readonly PropertyRecordComparer Instance = new();

    public bool Equals(PropertyRecord? x, PropertyRecord? y)
    {
        return x?.Equals(y) ?? y is null;
    }

    public int GetHashCode(PropertyRecord obj)
    {
        return obj.GetHashCode();
    }
}