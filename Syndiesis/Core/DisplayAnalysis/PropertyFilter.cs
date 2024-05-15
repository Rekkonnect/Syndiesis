using System;

namespace Syndiesis.Core.DisplayAnalysis;

public abstract class PropertyFilter
{
    public abstract PropertyFilterResult FilterProperties(Type type);
}
