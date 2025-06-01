namespace Syndiesis.ColorHelpers;

public readonly record struct HslTransformation(
    double Alpha = 0,
    double Hue = 0,
    double Saturation = 0,
    double Lightness = 0)
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
