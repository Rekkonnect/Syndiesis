namespace Syndiesis.ColorHelpers;

public sealed class FixedUpdatedSolidBrush : ILazilyUpdatedSolidBrush
{
    private readonly SolidColorBrush _brush;

    public readonly Color Color;

    public SolidColorBrush Brush => _brush;
    IBrush ILazilyUpdatedBrush.Brush => _brush;
    Color ILazilyUpdatedSolidBrush.Color => Color;

    public FixedUpdatedSolidBrush(Color color)
        : this(new SolidColorBrush(color))
    {
    }

    public FixedUpdatedSolidBrush(SolidColorBrush brush)
    {
        Color = brush.Color;
        _brush = brush;
    }

    public LazilyUpdatedHsvTransformedSolidBrush WithHsvTransformation(
        HsvTransformation transformation)
    {
        return new(this, transformation);
    }
}
