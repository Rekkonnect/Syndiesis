using Avalonia.Controls.Documents;
using Syndiesis.Core.DisplayAnalysis;
using System.Collections.Generic;
using System.Text;

namespace Syndiesis.Controls.Inlines;

public sealed class SingleRunInline : GroupedRunInline
{
    public Run Run { get; }

    public override IEnumerable<object> InlineObjects => [Run];

    public SingleRunInline(Run run, string? overrideText = null)
    {
        Run = run;
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

    public sealed record Builder(
        UIBuilder.Run Run,
        string? OverrideText = null)
        : Builder<SingleRunInline>(OverrideText)
    {
        public override SingleRunInline Build()
        {
            return new(Run.Build(), OverrideText);
        }
    }
}
