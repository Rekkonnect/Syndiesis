using Avalonia.Controls.Documents;
using System.Collections.Generic;
using System.Text;

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
}
