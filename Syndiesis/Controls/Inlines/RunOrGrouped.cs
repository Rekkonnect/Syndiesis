using Avalonia.Controls.Documents;
using Syndiesis.Core.DisplayAnalysis;

namespace Syndiesis.Controls.Inlines;

public readonly struct RunOrGrouped
{
    public readonly Run? Run
        => AvailableObject as Run;
    public readonly UIBuilder.Run? RunBuilder
        => AvailableObject as UIBuilder.Run;
    public readonly GroupedRunInline? Grouped
        => AvailableObject as GroupedRunInline;
    public readonly GroupedRunInline.IBuilder? GroupedBuilder
        => AvailableObject as GroupedRunInline.IBuilder;

    public object? AvailableObject { get; }
    public ValueKind ContainedValueKind { get; }

    public bool IsDefault => AvailableObject is null;

    public RunOrGrouped(Run run)
    {
        AvailableObject = run;
        ContainedValueKind = ValueKind.Run;
    }

    public RunOrGrouped(UIBuilder.Run run)
    {
        AvailableObject = run;
        ContainedValueKind = ValueKind.RunBuilder;
    }

    public RunOrGrouped(GroupedRunInline grouped)
    {
        AvailableObject = grouped;
        ContainedValueKind = ValueKind.GroupedRunInline;
    }

    public RunOrGrouped(GroupedRunInline.IBuilder grouped)
    {
        AvailableObject = grouped;
        ContainedValueKind = ValueKind.GroupedRunInlineBuilder;
    }

    public RunOrGrouped Build()
    {
        switch (ContainedValueKind)
        {
            case ValueKind.RunBuilder:
                return RunBuilder!.Build();
            case ValueKind.GroupedRunInlineBuilder:
                return GroupedBuilder!.Build();
        }

        return this;
    }

    public static implicit operator RunOrGrouped(Run run) => new(run);
    public static implicit operator RunOrGrouped(UIBuilder.Run run) => new(run);
    public static implicit operator RunOrGrouped(GroupedRunInline grouped) => new(grouped);
    public static implicit operator RunOrGrouped(SingleRunInline.Builder grouped) => new(grouped);
    public static implicit operator RunOrGrouped(SimpleGroupedRunInline.Builder grouped) => new(grouped);
    public static implicit operator RunOrGrouped(ComplexGroupedRunInline.Builder grouped) => new(grouped);

    public static RunOrGrouped FromObject(object? @object)
    {
        switch (@object)
        {
            case null:
                return default;

            case Run run:
                return new(run);

            case UIBuilder.Run runBuilder:
                return new(runBuilder);

            case GroupedRunInline grouped:
                return new(grouped);

            case GroupedRunInline.IBuilder groupedBuilder:
                return new(groupedBuilder);

            case RunOrGrouped runOrGrouped:
                return runOrGrouped;
        }

        throw new ArgumentException("Invalid object");
    }

    public enum ValueKind
    {
        Run,
        RunBuilder,
        GroupedRunInline,
        GroupedRunInlineBuilder,
    }
}
