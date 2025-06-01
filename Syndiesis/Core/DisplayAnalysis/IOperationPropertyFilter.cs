using Microsoft.CodeAnalysis;
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
            // Avoid recursion
            case nameof(IOperation.Parent):
            case nameof(IOperation.SemanticModel):
                return false;

            // An alternative method to access the contained operations,
            // which is redundant in our case
            case nameof(IOperation.ChildOperations):
                return false;

#pragma warning disable CS0618 // Type or member is obsolete

            // Remove obsolete members
            case nameof(IOperation.Children):
                return false;

#pragma warning restore CS0618 // Type or member is obsolete

            // The kind is displayed next to the operation's type
            case nameof(IOperation.Kind):
            // The language is inferred from the syntax tree
            case nameof(IOperation.Language):
                return false;

            // We like all other publicly-exposed properties of the API
            default:
                return true;
        }
    }
}
