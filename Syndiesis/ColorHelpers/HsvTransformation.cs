using Avalonia.Media;

namespace Syndiesis.ColorHelpers;

public readonly record struct HsvTransformation(double Alpha, double Hue, double Saturation, double Value)
    : IColorTransformation<HsvColor>
{
    public HsvColor Transform(HsvColor color)
    {
        return new(
            color.A + Alpha,
            color.H + Hue,
            color.S + Saturation,
            color.V + Value);
    }
    
    public Color TransformRgb(Color color)
    {
        return Transform(color.ToHsv()).ToRgb();
    }
}