using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Reflection;

namespace Syndiesis.Core.DisplayAnalysis;

public sealed class ISymbolPropertyFilter : PropertyFilter
{
    public static readonly ISymbolPropertyFilter Instance = new();

    public override PropertyFilterResult FilterProperties(Type type)
    {
        var properties = type.GetProperties();
        var interestingTypeProperties = properties.Where(FilterSymbolProperty)
            .ToArray();

        return new()
        {
            Properties = interestingTypeProperties
        };
    }

    private static bool FilterSymbolProperty(PropertyInfo propertyInfo)
    {
        var name = propertyInfo.Name;

        switch (name)
        {
            // Avoid recursion
            case nameof(ISymbol.ContainingAssembly):
            case nameof(ISymbol.ContainingModule):
            case nameof(ISymbol.ContainingNamespace):
            case nameof(ISymbol.ContainingSymbol):
            case nameof(ISymbol.ContainingType):
                return false;

            // We like all other publicly-exposed properties of the API
            default:
                return true;
        }
    }
}
