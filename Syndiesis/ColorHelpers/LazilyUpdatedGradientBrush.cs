namespace Syndiesis.ColorHelpers;

public sealed class LazilyUpdatedGradientBrush : ILazilyUpdatedBrush
{
    private readonly LinearGradientBrush _brush;
    private IReadOnlyList<LazilyUpdatedGradientStop> _stops = [];

    private LazilyUpdatedGradientBrush(
        LinearGradientBrush brush)
    {
        _brush = brush;
    }

    public LazilyUpdatedGradientBrush(
        LinearGradientBrush brush, IReadOnlyList<LazilyUpdatedGradientStop> stops)
    {
        _brush = brush;
        GradientStops = stops;
    }

    public IReadOnlyList<LazilyUpdatedGradientStop> GradientStops
    {
        get => _stops;
        set
        {
            _stops = value;
            _brush.GradientStops = [.. _stops.Select(s => s.Stop)];
        }
    }

    public LinearGradientBrush Brush
    {
        get
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                foreach (var stop in _stops)
                {
                    stop.Update();
                }
            }
            return _brush;
        }
    }

    IBrush ILazilyUpdatedBrush.Brush => Brush;

    public static LazilyUpdatedGradientBrush FromBuiltStops(LinearGradientBrush brush)
    {
        var stops = brush.GradientStops;
        var lazyStops = stops
            .Select(s => new LazilyUpdatedGradientStop(s))
            .ToList();
        var result = new LazilyUpdatedGradientBrush(brush);
        result._stops = lazyStops;
        return result;
    }
}
