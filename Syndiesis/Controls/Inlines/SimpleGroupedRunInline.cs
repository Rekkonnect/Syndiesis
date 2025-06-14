﻿using Avalonia.Controls.Documents;
using Syndiesis.Core.DisplayAnalysis;

namespace Syndiesis.Controls.Inlines;

public sealed class SimpleGroupedRunInline : GroupedRunInline
{
    public List<Run> Children { get; set; } = new();

    public override IEnumerable<object> InlineObjects => Children;

    public SimpleGroupedRunInline() { }

    public SimpleGroupedRunInline(IEnumerable<Run> children)
    {
        Children.AddRange(children);
    }

    protected override int CalculatedTextLength()
    {
        int length = 0;
        foreach (var run in Children)
        {
            length += run.Text?.Length ?? 0;
        }
        return length;
    }

    protected override void CalculateText(StringBuilder builder)
    {
        foreach (var run in Children)
        {
            builder.Append(run.Text);
        }
    }

    public override void AppendToInlines(InlineCollection inlines)
    {
        inlines.AddRange(Children);
    }

    public sealed class Builder(
        List<UIBuilder.Run>? children = null)
        : Builder<SimpleGroupedRunInline>
    {
        public List<UIBuilder.Run>? Children { get; set; } = children;
        
        public override SimpleGroupedRunInline Build()
        {
            if (Children is null)
                return new();

            var built = Children.Select(s => s.Build());
            return new(built);
        }
    }
}
