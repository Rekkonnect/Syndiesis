using Avalonia.Media;

namespace Syndiesis.ColorHelpers;

public readonly record struct HslTransformation(double Alpha, double Hue, double Saturation, double Lightness)
    : IColorTransformation<HslColor>
{
    public HslColor Transform(HslColor color)
    {
        return new(
            color.A + Alpha,
            color.H + Hue,
            color.S + Saturation,
            color.L + Lightness);
    }
    
    public Color TransformRgb(Color color)
    {
        return Transform(color.ToHsl()).ToRgb();
    }
}
