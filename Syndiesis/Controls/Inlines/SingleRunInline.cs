using Avalonia.Controls.Documents;
using System.Collections.Generic;
using System.Text;

namespace Syndiesis.Controls.Inlines;

public sealed class SingleRunInline(Run run) : GroupedRunInline
{
    public Run Run { get; } = run;

    public override IEnumerable<object> InlineObjects => [Run];

    public SingleRunInline(Run run, string overrideText)
        : this(run)
    {
        OverrideText = overrideText;
    }

    public override void AppendToInlines(InlineCollection inlines)
    {
        inlines.Add(Run);
    }

    protected override int CalculatedTextLength()
    {
        return Run.Text!.Length;
    }

    protected override void CalculateText(StringBuilder builder)
    {
        builder.Append(Run.Text);
    }
}
