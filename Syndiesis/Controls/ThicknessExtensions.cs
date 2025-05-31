using Avalonia;

namespace Syndiesis.Controls;

// Should be part of the public API or a third-party library
public static class ThicknessExtensions
{
    public static Thickness WithLeft(this Thickness thickness, double left)
    {
        return new(left, thickness.Top, thickness.Right, thickness.Bottom);
    }
    public static Thickness WithTop(this Thickness thickness, double top)
    {
        return new(thickness.Left, top, thickness.Right, thickness.Bottom);
    }
    public static Thickness WithRight(this Thickness thickness, double right)
    {
        return new(thickness.Left, thickness.Top, right, thickness.Bottom);
    }
    public static Thickness WithBottom(this Thickness thickness, double bottom)
    {
        return new(thickness.Left, thickness.Top, thickness.Right, bottom);
    }

    public static Thickness WithVertical(this Thickness thickness, double vertical)
    {
        return new(thickness.Left, top: vertical, thickness.Right, bottom: vertical);
    }
    public static Thickness WithHorizontal(this Thickness thickness, double horizontal)
    {
        return new(left: horizontal, thickness.Top, right: horizontal, thickness.Bottom);
    }

    public static Thickness KeepLeft(this Thickness thickness)
    {
        return new(thickness.Left, 0, 0, 0);
    }
    public static Thickness KeepTop(this Thickness thickness)
    {
        return new(0, thickness.Top, 0, 0);
    }
    public static Thickness KeepRight(this Thickness thickness)
    {
        return new(0, 0, thickness.Right, 0);
    }
    public static Thickness KeepBottom(this Thickness thickness)
    {
        return new(0, 0, 0, thickness.Bottom);
    }

    public static Thickness KeepHorizontal(this Thickness thickness, double horizontal)
    {
        return new(thickness.Left, 0, thickness.Right, 0);
    }
    public static Thickness KeepVertical(this Thickness thickness, double vertical)
    {
        return new(0, thickness.Top, 0, thickness.Bottom);
    }

    public static Thickness OffsetLeft(this Thickness thickness, double left)
    {
        return new(thickness.Left + left, thickness.Top, thickness.Right, thickness.Bottom);
    }
    public static Thickness OffsetTop(this Thickness thickness, double top)
    {
        return new(thickness.Left, thickness.Top + top, thickness.Right, thickness.Bottom);
    }
    public static Thickness OffsetRight(this Thickness thickness, double right)
    {
        return new(thickness.Left, thickness.Top, thickness.Right + right, thickness.Bottom);
    }
    public static Thickness OffsetBottom(this Thickness thickness, double bottom)
    {
        return new(thickness.Left, thickness.Top, thickness.Right, thickness.Bottom + bottom);
    }

    public static double CommonHorizontal(this Thickness thickness)
    {
        return CommonOrNaN(thickness.Left, thickness.Right);
    }

    public static double CommonVertical(this Thickness thickness)
    {
        return CommonOrNaN(thickness.Top, thickness.Bottom);
    }

    public static double CommonAll(this Thickness thickness)
    {
        var top = thickness.Top;
        if (top != thickness.Right
            || thickness.Bottom != thickness.Left
            || top != thickness.Bottom)
        {
            return double.NaN;
        }

        return top;
    }

    private static double CommonOrNaN(double left, double right)
    {
        if (left != right)
        {
            return double.NaN;
        }

        return left;
    }
}
