using Avalonia.Media;
using Avalonia.Threading;

namespace Syndiesis;

public sealed class LazilyUpdatedSolidBrush : ILazilyUpdatedBrush
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

    public LazilyUpdatedSolidBrush() { }

    public LazilyUpdatedSolidBrush(Color color)
    {
        Color = color;
    }
}
