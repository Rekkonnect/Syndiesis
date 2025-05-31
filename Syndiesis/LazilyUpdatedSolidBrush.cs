using Syndiesis.ColorHelpers;

namespace Syndiesis;

// TODO Move to .Colors
public sealed class LazilyUpdatedSolidBrush : ILazilyUpdatedSolidBrush
{
    private readonly SolidColorBrush _brush = new();

    public Color Color;

    public SolidColorBrush Brush
    {
        get
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                _brush.Color = Color;
            }

            return _brush;
        }
    }

    IBrush ILazilyUpdatedBrush.Brush => Brush;
    Color ILazilyUpdatedSolidBrush.Color => Color;

    public LazilyUpdatedSolidBrush()
    {
    }

    public LazilyUpdatedSolidBrush(Color color)
    {
        Color = color;
    }

    public LazilyUpdatedHsvTransformedSolidBrush WithHsvTransformation(HsvTransformation transformation)
    {
        return new(this, transformation);
    }
}
