using System.Collections.Generic;
using System.Reflection;

namespace Syndiesis.Core.DisplayAnalysis;

public class PropertyFilterResult
{
    public required IReadOnlyList<PropertyInfo> Properties { get; init; }
}
