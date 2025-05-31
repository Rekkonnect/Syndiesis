using Avalonia;
using Avalonia.Controls;
using System;

namespace Syndiesis.Controls;

public class Margin
{
    public static readonly AttachedProperty<double> LeftProperty =
        AvaloniaProperty.RegisterAttached<Margin, Control, double>(
            "Left", double.NaN);

    public static readonly AttachedProperty<double> TopProperty =
        AvaloniaProperty.RegisterAttached<Margin, Control, double>(
            "Top", double.NaN);

    public static readonly AttachedProperty<double> RightProperty =
        AvaloniaProperty.RegisterAttached<Margin, Control, double>(
            "Right", double.NaN);

    public static readonly AttachedProperty<double> BottomProperty =
        AvaloniaProperty.RegisterAttached<Margin, Control, double>(
            "Bottom", double.NaN);

    public static readonly AttachedProperty<double> VerticalProperty =
        AvaloniaProperty.RegisterAttached<Margin, Control, double>(
            "Vertical", double.NaN);

    public static readonly AttachedProperty<double> HorizontalProperty =
        AvaloniaProperty.RegisterAttached<Margin, Control, double>(
            "Horizontal", double.NaN);

    public static double GetTop(Control control)
    {
        return control.Margin.Top;
    }

    public static double GetLeft(Control control)
    {
        return control.Margin.Left;
    }

    public static double GetBottom(Control control)
    {
        return control.Margin.Bottom;
    }

    public static double GetRight(Control control)
    {
        return control.Margin.Right;
    }

    public static double GetHorizontal(Control control)
    {
        return control.Margin.CommonHorizontal();
    }

    public static double GetVertical(Control control)
    {
        return control.Margin.CommonVertical();
    }

    public static void SetTop(Control control, double value)
    {
        MutateMargin(control, value, ThicknessExtensions.WithTop);
    }

    public static void SetRight(Control control, double value)
    {
        MutateMargin(control, value, ThicknessExtensions.WithRight);
    }

    public static void SetBottom(Control control, double value)
    {
        MutateMargin(control, value, ThicknessExtensions.WithBottom);
    }

    public static void SetLeft(Control control, double value)
    {
        MutateMargin(control, value, ThicknessExtensions.WithLeft);
    }

    public static void SetVertical(Control control, double value)
    {
        MutateMargin(control, value, ThicknessExtensions.WithVertical);
    }

    public static void SetHorizontal(Control control, double value)
    {
        MutateMargin(control, value, ThicknessExtensions.WithHorizontal);
    }

    private static void MutateMargin(
        Control control,
        double component,
        Func<Thickness, double, Thickness> mutation)
    {
        var margin = control.Margin;
        control.Margin = mutation(margin, component);
    }
}
