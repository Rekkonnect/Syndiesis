namespace Syndiesis.ColorHelpers;

public sealed class LazilyUpdatedGradientStop(GradientStop stop)
{
    private readonly GradientStop _stop = stop;

    public Color Color;

    public GradientStop Stop
    {
        get
        {
            Update();
            return _stop;
        }
    }

    public LazilyUpdatedGradientStop()
        : this(new GradientStop()) { }

    public void Update()
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            _stop.Color = Color;
        }
    }
}
