using Avalonia;

namespace Syndiesis.Utilities;

public static class ComponentExtensions
{
    public static Thickness WithLeft(this Thickness thickness, double left)
    {
        return new(left, thickness.Top, thickness.Right, thickness.Bottom);
    }
    public static Thickness WithRight(this Thickness thickness, double right)
    {
        return new(thickness.Left, thickness.Top, right, thickness.Bottom);
    }
    public static Thickness WithTop(this Thickness thickness, double top)
    {
        return new(thickness.Left, top, thickness.Right, thickness.Bottom);
    }
    public static Thickness WithBottom(this Thickness thickness, double bottom)
    {
        return new(thickness.Left, thickness.Top, thickness.Right, bottom);
    }
}
