using Microsoft.CodeAnalysis;
using Syndiesis.Controls.AnalysisVisualization;
using System;
using System.Collections.Generic;
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

            // We like all other publicly-exposed properties of the API
            default:
                return true;
        }
    }
}
