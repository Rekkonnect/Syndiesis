namespace Syndiesis.ColorHelpers;

public static class ColorTransformationExtensions
{
    public static Color TransformHsv(this Color color, HsvTransformation transformation)
    {
        return transformation.TransformRgb(color);
    }

    public static Color TransformHsl(this Color color, HslTransformation transformation)
    {
        return transformation.TransformRgb(color);
    }

    public static Color Transform<TTransformation>(
        this Color color,
        TTransformation transformation)
        where TTransformation : IColorTransformation
    {
        return transformation.TransformRgb(color);
    }
}
