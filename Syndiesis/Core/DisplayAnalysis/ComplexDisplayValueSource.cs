using System.Collections.Generic;

namespace Syndiesis.Core.DisplayAnalysis;

public sealed record class ComplexDisplayValueSource(
    DisplayValueSource Value, ComplexDisplayValueSource? Child)
{
    public DisplayValueSource.SymbolKind Modifiers { get; set; }

    public IEnumerable<DisplayValueSource> ChildrenValueSources()
    {
        var current = this;
        while (current is not null)
        {
            yield return current.Value;
            current = current.Child;
        }
    }

    public DisplayValueSource DeepestChild()
    {
        var current = this;
        while (true)
        {
            var child = current.Child;
            if (child is null)
                return current.Value;

            current = child;
        }
    }
}
