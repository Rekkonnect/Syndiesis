using System.Reflection;

namespace Syndiesis.Core.DisplayAnalysis;

public class PropertyFilterResult
{
    public static readonly PropertyFilterResult Empty = new() { Properties = [] };

    public required IReadOnlyList<PropertyInfo> Properties { get; init; }
    public Type? PreferredType { get; init; }
}
