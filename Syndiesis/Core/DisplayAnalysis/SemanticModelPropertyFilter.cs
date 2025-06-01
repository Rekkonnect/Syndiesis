using Microsoft.CodeAnalysis;
using System.Reflection;

namespace Syndiesis.Core.DisplayAnalysis;

public sealed class SemanticModelPropertyFilter : PropertyFilter
{
    public static readonly SemanticModelPropertyFilter Instance = new();

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
            case nameof(SemanticModel.Compilation):
            case nameof(SemanticModel.ParentModel):
            case nameof(SemanticModel.SyntaxTree):

            // Unimportant or overly specific properties
            case nameof(SemanticModel.OriginalPositionForSpeculation):
                return false;

            default:
                return true;
        }
    }
}
