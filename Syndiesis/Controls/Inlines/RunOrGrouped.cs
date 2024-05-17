using Avalonia.Controls.Documents;
using System;

namespace Syndiesis.Controls.Inlines;

public readonly struct RunOrGrouped
{
    public readonly Run? Run;
    public readonly GroupedRunInline? Grouped;

    public object? AvailableObject => Run ?? Grouped as object;

    public bool IsDefault => AvailableObject is null;

    public RunOrGrouped(Run run)
    {
        Run = run;
    }

    public RunOrGrouped(GroupedRunInline grouped)
    {
        Grouped = grouped;
    }

    public static implicit operator RunOrGrouped(Run run) => new(run);
    public static implicit operator RunOrGrouped(GroupedRunInline grouped) => new(grouped);

    public static RunOrGrouped FromObject(object? @object)
    {
        switch (@object)
        {
            case null:
                return default;

            case Run run:
                return new(run);

            case GroupedRunInline grouped:
                return new(grouped);
        }

        throw new ArgumentException("Invalid object");
    }
}
