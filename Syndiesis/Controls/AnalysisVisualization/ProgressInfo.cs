namespace Syndiesis.Controls.AnalysisVisualization;

public readonly record struct ProgressInfo(int Value, int Maximum)
{
    public static readonly ProgressInfo Invalid = new(0, 0);

    public bool IsValid => Maximum > 0;

    public int RealValue => Math.Min(Value, Maximum);

    public double Rate => IsValid ? (double)RealValue / Maximum : 0;
}
