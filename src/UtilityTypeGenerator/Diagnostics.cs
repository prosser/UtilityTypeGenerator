namespace UtilityTypeGenerator;
using Microsoft.CodeAnalysis;

internal static class Diagnostics
{
    public static DiagnosticDescriptor InvalidSelector { get; } = new(
        id: "UTG0001",
        title: "Invalid selector",
        messageFormat: "Invalid selector: {0}. Expected Pick<T, P1|P2>, Omit<T, P1|P2>, Import<T>, Union<T1,T2>, Intersection<T1,T2>, Readonly<T>, Required<T>, Optional<T>, or Nullable<T>",
        category: "UtilityTypeGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}