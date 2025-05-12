using Avalonia.Media;
using Avalonia.Threading;

namespace Syndiesis.ColorHelpers;

public abstract class LazilyUpdatedTransformedSolidBrush<TColorTransformation, TColor>(
    LazilyUpdatedSolidBrush mainSolid,
    TColorTransformation transformation)
    : ILazilyUpdatedBrush
    where TColorTransformation : IColorTransformation<TColor> 
{
    private readonly LazilyUpdatedSolidBrush _mainSolid = mainSolid;
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
