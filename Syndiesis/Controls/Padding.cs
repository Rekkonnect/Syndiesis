using Avalonia;
using Avalonia.Controls;
using System;

namespace Syndiesis.Controls;

public class Padding
{
    public static readonly AttachedProperty<double> LeftProperty =
        AvaloniaProperty.RegisterAttached<Padding, Control, double>(
            "Left", double.NaN);

    public static readonly AttachedProperty<double> TopProperty =
        AvaloniaProperty.RegisterAttached<Padding, Control, double>(
            "Top", double.NaN);

    public static readonly AttachedProperty<double> RightProperty =
        AvaloniaProperty.RegisterAttached<Padding, Control, double>(
            "Right", double.NaN);

    public static readonly AttachedProperty<double> BottomProperty =
        AvaloniaProperty.RegisterAttached<Padding, Control, double>(
            "Bottom", double.NaN);

    public static readonly AttachedProperty<double> VerticalProperty =
        AvaloniaProperty.RegisterAttached<Padding, Control, double>(
            "Vertical", double.NaN);

    public static readonly AttachedProperty<double> HorizontalProperty =
        AvaloniaProperty.RegisterAttached<Padding, Control, double>(
            "Horizontal", double.NaN);

    public static double GetTop(Control control)
    {
        return control.GetPadding()?.Top ?? double.NaN;
    }

    public static double GetLeft(Control control)
    {
        return control.GetPadding()?.Left ?? double.NaN;
    }

    public static double GetBottom(Control control)
    {
        return control.GetPadding()?.Bottom ?? double.NaN;
    }

    public static double GetRight(Control control)
    {
        return control.GetPadding()?.Right ?? double.NaN;
    }

    public static double GetHorizontal(Control control)
    {
        return control.GetPadding()?.CommonHorizontal() ?? double.NaN;
    }

    public static double GetVertical(Control control)
    {
        return control.GetPadding()?.CommonVertical() ?? double.NaN;
    }

    public static void SetTop(Control control, double value)
    {
        MutatePadding(control, value, ThicknessExtensions.WithTop);
    }

    public static void SetRight(Control control, double value)
    {
        MutatePadding(control, value, ThicknessExtensions.WithRight);
    }

    public static void SetBottom(Control control, double value)
    {
        MutatePadding(control, value, ThicknessExtensions.WithBottom);
    }

    public static void SetLeft(Control control, double value)
    {
        MutatePadding(control, value, ThicknessExtensions.WithLeft);
    }

    public static void SetVertical(Control control, double value)
    {
        MutatePadding(control, value, ThicknessExtensions.WithVertical);
    }

    public static void SetHorizontal(Control control, double value)
    {
        MutatePadding(control, value, ThicknessExtensions.WithHorizontal);
    }

    private static void MutatePadding(
        Control control,
        double component,
        Func<Thickness, double, Thickness> mutation)
    {
        var padding = control.GetPadding();
        if (padding is null)
        {
            return;
        }

        control.SetPadding(mutation(padding.Value, component));
    }
}
