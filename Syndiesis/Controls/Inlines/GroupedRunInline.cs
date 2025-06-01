using Avalonia.Controls.Documents;
using Syndiesis.Core.DisplayAnalysis;

namespace Syndiesis.Controls.Inlines;

public abstract class GroupedRunInline
{
    public abstract IEnumerable<object> InlineObjects { get; }

    public string? OverrideText { get; set; }

    public string EffectiveText()
    {
        if (OverrideText is not null)
            return OverrideText;

        var builder = new StringBuilder();
        AppendEffectiveText(builder);
        return builder.ToString();
    }

    public int EffectiveTextLength()
    {
        if (OverrideText is not null)
            return OverrideText.Length;

        return CalculatedTextLength();
    }

    public void AppendEffectiveText(StringBuilder builder)
    {
        if (OverrideText is not null)
        {
            builder.Append(OverrideText);
            return;
        }

        CalculateText(builder);
    }

    protected abstract int CalculatedTextLength();

    protected abstract void CalculateText(StringBuilder builder);

    public abstract void AppendToInlines(InlineCollection inlines);

    public static int GetEffectiveTextLength(RunOrGrouped runOrGrouped)
    {
        if (runOrGrouped.Run is not null and var run)
        {
            return run.Text!.Length;
        }
        else if (runOrGrouped.Grouped is not null and var grouped)
        {
            return grouped.EffectiveTextLength();
        }

        return 0;
    }

    public static int GetTextLength(RunOrGrouped runOrGrouped)
    {
        if (runOrGrouped.Run is not null and var run)
        {
            return run.Text!.Length;
        }
        else if (runOrGrouped.Grouped is not null and var grouped)
        {
            return grouped.CalculatedTextLength();
        }

        return 0;
    }

    public static void AppendText(StringBuilder builder, RunOrGrouped runOrGrouped)
    {
        if (runOrGrouped.Run is not null and var run)
        {
            builder.Append(run.Text);
        }
        else if (runOrGrouped.Grouped is not null and var grouped)
        {
            grouped.AppendEffectiveText(builder);
        }
    }

    public static void AppendToInlines(InlineCollection inlines, RunOrGrouped runOrGrouped)
    {
        if (runOrGrouped.Run is not null and var run)
        {
            inlines.Add(run);
        }
        else if (runOrGrouped.Grouped is not null and var grouped)
        {
            grouped.AppendToInlines(inlines);
        }
    }

    public interface IBuilder
    {
        public GroupedRunInline Build();

        public sealed RunOrGrouped AsRunOrGrouped => new(this);
    }

    public abstract class Builder<T>
        : UIBuilder<T>, IBuilder
        where T : GroupedRunInline
    {
        GroupedRunInline IBuilder.Build()
        {
            return Build();
        }
    }
}
