namespace UtilityTypeGenerator;

using Microsoft.CodeAnalysis;

internal static class Diagnostics
{
    public static DiagnosticDescriptor InvalidSelector => new(
        id: "UTG0001",
        title: "Invalid selector",
        messageFormat: "Invalid selector: {0}. Expected Pick<T, P1|P2>, Omit<T, P1|P2>, Import<T>, Union<T1,T2>, Intersection<T1,T2>, Readonly<T>, Required<T>, Optional<T>, or Nullable<T>",
        category: "UtilityTypeGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor TypeError => new(
        id: "UTG0002",
        title: "Type error",
        messageFormat: "Type error in selector \"{0}\": {1}",
        category: "UtilityTypeGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor UnknownError => new(
        id: "UTG0003",
        title: "Unknown error",
        messageFormat: "Unknown error in selector \"{0}\": {1}",
        category: "UtilityTypeGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor InvalidPropertyName => new(
        id: "UTG0004",
        title: "Invalid property name",
        messageFormat: "Invalid property name in selector \"{0}\": {1} was not found",
        category: "UtilityTypeGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}