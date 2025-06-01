namespace Syndiesis.ColorHelpers;

public abstract class LazilyUpdatedTransformedSolidBrush<TColorTransformation, TColor>(
    ILazilyUpdatedSolidBrush mainSolid,
    TColorTransformation transformation)
    : ILazilyUpdatedBrush
    where TColorTransformation : IColorTransformation<TColor> 
{
    private readonly ILazilyUpdatedSolidBrush _mainSolid = mainSolid;
    private readonly SolidColorBrush _brush = new();

    public TColorTransformation Transformation = transformation;

    public SolidColorBrush Brush
    {
        get
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                _brush.Color = GetColor();
            }

            return _brush;
        }
    }

    IBrush ILazilyUpdatedBrush.Brush => Brush;

    private Color GetColor()
    {
        return Transformation.TransformRgb(_mainSolid.Color);
    }
}
