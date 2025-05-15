using Avalonia.Media;

namespace Syndiesis.ColorHelpers;

public readonly record struct HsvTransformation(
    double Alpha = 0,
    double Hue = 0,
    double Saturation = 0,
    double Value = 0)
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