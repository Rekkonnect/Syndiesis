using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Reflection;

namespace Syndiesis.Core.DisplayAnalysis;

public sealed class IOperationPropertyFilter : PropertyFilter
{
    public static readonly IOperationPropertyFilter Instance = new();

    public override PropertyFilterResult FilterProperties(Type type)
    {
        var properties = type.GetProperties();
        var interestingTypeProperties = properties.Where(FilterOperationProperty)
            .ToArray();

        return new()
        {
            Properties = interestingTypeProperties
        };
    }

    private static bool FilterOperationProperty(PropertyInfo propertyInfo)
    {
        var name = propertyInfo.Name;

        switch (name)
        {
            // Explicitly return false on this to avoid being caught by the
            // property return value filter below
            case nameof(IOperation.Parent):
                return false;

#pragma warning disable CS0618 // Type or member is obsolete

            // Reomve obsolete members
            case nameof(IOperation.Children):
                return false;

#pragma warning restore CS0618 // Type or member is obsolete

            // We like all other publicly-exposed properties of the API
            default:
                return true;
        }
    }
}
