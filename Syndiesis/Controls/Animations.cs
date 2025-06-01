using Avalonia.Animation;
using Avalonia.Styling;

namespace Syndiesis.Controls;

public static class Animations
{
    public static Animation CreateColorPulseAnimation(
        Control control,
        Color fillColor,
        AvaloniaProperty<IBrush?> colorProperty)
    {
        return CreatePropertyPulseAnimation(
            control,
            new SolidColorBrush(fillColor),
            colorProperty);
    }

    public static Animation CreatePropertyPulseAnimation<T>(
        Control control,
        T? pulseValue,
        AvaloniaProperty<T> property)
    {
        return new()
        {
            Children =
            {
                SimpleKeyFrame(
                    cueValue: 0,
                    property: property,
                    value: pulseValue),
                SimpleKeyFrame(
                    cueValue: 1,
                    property: property,
                    value: control.GetValue<T>(property)),
            }
        };
    }

    public static KeyFrame SimpleKeyFrame<T>(
        double cueValue, AvaloniaProperty<T> property, T? value)
    {
        return new()
        {
            Cue = new(cueValue),
            Setters =
            {
                new Setter(property, value)
            },
        };
    }
}
