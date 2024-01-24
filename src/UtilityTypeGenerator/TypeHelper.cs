namespace UtilityTypeGenerator;

using System;
using Microsoft.CodeAnalysis;

internal static class TypeHelper
{
    public static PropertyRecord[] GetPropertyRecords(this ITypeSymbol containingType)
    {
        return containingType.GetMembers()
            .OfType<IPropertySymbol>()
            .Select(symbol =>
            {
                bool isNullable = symbol.Type.IsNullable();
                return new PropertyRecord(symbol.Name, symbol.Type, isNullable, symbol.IsReadOnly, symbol.IsRequired);
            })
            .ToArray();
    }

    public static string? GetSimpleName(this Type type)
    {
        if (type == typeof(object))
        {
            return "object";
        }
        else if (type == typeof(bool))
        {
            return "bool";
        }
        else if (type == typeof(string))
        {
            return "string";
        }
        else if (type == typeof(int))
        {
            return "int";
        }
        else if (type == typeof(long))
        {
            return "long";
        }
        else if (type == typeof(float))
        {
            return "float";
        }
        else if (type == typeof(double))
        {
            return "double";
        }
        else if (type == typeof(decimal))
        {
            return "decimal";
        }
        else if (type == typeof(short))
        {
            return "short";
        }
        else if (type == typeof(uint))
        {
            return "uint";
        }
        else if (type == typeof(ulong))
        {
            return "ulong";
        }
        else if (type == typeof(ushort))
        {
            return "ushort";
        }
        else if (type == typeof(sbyte))
        {
            return "sbyte";
        }
        else if (type == typeof(byte))
        {
            return "byte";
        }
        else if (type == typeof(char))
        {
            return "char";
        }

        return null;
    }

    public static IEnumerable<INamedTypeSymbol> GetTypesByMetadataName(this Compilation compilation, string typeMetadataName)
    {
        return compilation.References
            .Select(compilation.GetAssemblyOrModuleSymbol)
            .OfType<IAssemblySymbol>()
            .Select(assemblySymbol => assemblySymbol.GetTypeByMetadataName(typeMetadataName))
            .Where(t => t is not null)
            .Cast<INamedTypeSymbol>();
    }

    public static IEnumerable<INamedTypeSymbol> GetTypesByName(this Compilation compilation, string? typeName, string[]? usingNamespaces = null)
    {
        if (typeName is null)
        {
            yield break;
        }

        (string[] namespaceParts, string name, int arity) = DeconstructTypeName(typeName);
        if (namespaceParts.Length > 0 && usingNamespaces?.Length > 0)
        {
            foreach (string usingNamespace in usingNamespaces)
            {
                foreach (INamedTypeSymbol t in compilation.GetTypesByName($"{usingNamespace}.{typeName}"))
                {
                    yield return t;
                }
            }

            yield break;
        }

        bool anyNamespace = namespaceParts.Length == 0;

        INamespaceSymbol globalNamespace = compilation.GlobalNamespace;

        if (anyNamespace)
        {
            IEnumerable<INamedTypeSymbol> GetTypesInAnyNamespace(INamespaceSymbol ns)
            {
                return ns
                    .GetTypeMembers(name).Where(x => x.Arity == arity)
                    .Concat(ns.GetNamespaceMembers().SelectMany(n => GetTypesInAnyNamespace(n)));
            }

            foreach (INamedTypeSymbol t in GetTypesInAnyNamespace(globalNamespace))
            {
                yield return t;
            }
        }
        else
        {
            INamespaceSymbol? ns = globalNamespace;
            foreach (string part in namespaceParts)
            {
                ns = ns?.GetNamespaceMembers().SingleOrDefault(x => x.Name == part);
            }

            if (ns is null)
            {
                yield break;
            }

            yield return ns.GetTypesByName(name).SingleOrDefault(x => x.Arity == arity);
        }
    }

    public static IEnumerable<INamedTypeSymbol> GetTypesByName(this INamespaceSymbol namespaceSymbol, string typeName)
    {
        return namespaceSymbol.GetTypeMembers(typeName)
            .Where(t => t is not null)
            .Concat(namespaceSymbol.GetNamespaceMembers().SelectMany(ns => ns.GetTypesByName(typeName)));
    }

    public static bool IsNullable(this ITypeSymbol type)
    {
        return type.IsNullableOfT() ||
            type.NullableAnnotation switch
            {
                NullableAnnotation.Annotated => true,
                NullableAnnotation.NotAnnotated => false,
                NullableAnnotation.None => type.IsReferenceType && !type.IsValueType(),
                _ => throw new NotSupportedException()
            };
    }

    public static bool IsNullableEquals(this ITypeSymbol type, ITypeSymbol other)
    {
        return type.IsNullableOfT() &&
            other.IsNullableOfT() &&
            SymbolEqualityComparer.Default.Equals(((INamedTypeSymbol)type).TypeArguments[0], ((INamedTypeSymbol)other).TypeArguments[0]);
    }

    public static bool IsNullableOfT(this ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedTypeSymbol &&
            namedTypeSymbol.IsGenericType)
        {
            ITypeSymbol nullableT = type.OriginalDefinition;
            return nullableT?.Name == nameof(Nullable) && nullableT.ContainingNamespace?.Name == nameof(System);
        }

        return false;
    }

    public static T MakeNotNull<T>(this T type)
        where T : ITypeSymbol
    {
        return type is INamedTypeSymbol namedTypeSymbol &&
            namedTypeSymbol.IsGenericType &&
            namedTypeSymbol.ConstructedFrom.IsNullableOfT()
            ? (T)namedTypeSymbol.TypeArguments[0]
            : type;
    }

    public static T MakeNullable<T>(this T type, Compilation compilation)
                                            where T : ITypeSymbol
    {
        return type.IsNullable()
            ? type
            : type.IsValueType()
                ? (T)compilation.GetTypeByMetadataName("System.Nullable`1")!.Construct(type)
                : (T)type.WithNullableAnnotation(NullableAnnotation.Annotated);
    }

    private static (string[] NamespaceParts, string Name, int Arity) DeconstructTypeName(string typeName)
    {
        int genericStartIndex = typeName.IndexOf('<');
        int namespaceEndIndex = genericStartIndex == -1
            ? typeName.LastIndexOf('.')
            : typeName.LastIndexOf('.', genericStartIndex);

        int arity = 0;
        if (genericStartIndex != -1)
        {
            arity = 1;
            int nesting = 0;
            foreach (char c in typeName[(genericStartIndex + 1)..])
            {
                switch (c)
                {
                    case '<':
                        nesting++;
                        break;

                    case '>':
                        nesting--;
                        break;

                    case ',' when nesting == 0:
                        arity++;
                        break;
                }
            }
        }

        return (
            namespaceEndIndex == -1 ? [] : typeName[..namespaceEndIndex].Split('.'),
            typeName[(namespaceEndIndex + 1)..],
            arity
            );
    }

    private static bool IsValueType(this ITypeSymbol type)
    {
        return type.BaseType?.Name == "ValueType" &&
            type.BaseType.ContainingNamespace.Name == "System";
    }

    //public static PropertyRecord GetPropertyRecord<T>(Expression<Func<T, object?>> expr)
    //{
    //    MemberExpression memberExpression = expr.Body as MemberExpression
    //        ?? (expr.Body is UnaryExpression unary ? unary.Operand as MemberExpression : throw new InvalidOperationException("Cannot get property from expression. Are you sure your expression is selecting a Property and not something else?"))
    //        ?? throw new InvalidCastException();
    //    return new(memberExpression.Member.Name, memberExpression.Type, memberExpression.Member.IsNullable());
    //}

    //public static PropertyRecord[] GetPropertyRecords<T>()
    //{
    //    return typeof(T).GetPropertyRecords();
    //}
    //public static PropertyRecord[] GetPropertyRecords(this Type type)
    //{
    //    return type.GetProperties()
    //        .Select(p => new PropertyRecord(p.Name, p.PropertyType, p.IsNullable()))
    //        .ToArray();
    //}
}