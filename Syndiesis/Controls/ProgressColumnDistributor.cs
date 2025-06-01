namespace Syndiesis.Controls;

public sealed record ProgressColumnDistributor(
    ColumnDefinition Progressed,
    ColumnDefinition Remaining)
{
    public void SetProgressRatio(double ratio)
    {
        Progressed.Width = CreateRatioLength(ratio);
        Remaining.Width = CreateRatioLength(1 - ratio);
    }

    private static GridLength CreateRatioLength(double ratio)
    {
        return new(ratio, GridUnitType.Star);
    }
}
