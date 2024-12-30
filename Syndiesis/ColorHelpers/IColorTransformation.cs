using Avalonia.Media;

namespace Syndiesis.ColorHelpers;

public interface IColorTransformation<TColor>
{
    public TColor Transform(TColor color);
    public Color TransformRgb(Color color);
}
