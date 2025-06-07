namespace Syndiesis.ColorHelpers;

public interface IColorTransformation
{
    public Color TransformRgb(Color color);
}

public interface IColorTransformation<TColor>
    : IColorTransformation
{
    public TColor Transform(TColor color);
}
