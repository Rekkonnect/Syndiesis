using System;
using System.Collections.Generic;

namespace Syndiesis.Core.DisplayAnalysis;

public sealed class InterestingPropertyFilterCache(PropertyFilter filter)
{
    private readonly PropertyFilter _filter = filter;

    private readonly Dictionary<Type, PropertyFilterResult> _filtered = new();

    public PropertyFilterResult FilterForType(Type type)
    {
        var contained = _filtered.TryGetValue(type, out var value);
        if (contained)
            return value!;

        return ForceFilter(type);
    }

    private PropertyFilterResult ForceFilter(Type type)
    {
        var result = _filter.FilterProperties(type);
        _filtered[type] = result;
        return result;
    }
}
