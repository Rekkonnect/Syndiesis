using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace Syndiesis.Controls.Inlines;

public static class Animations
{
    public static Animation CreateColorPulseAnimation(
        Control control,
        Color fillColor,
        AvaloniaProperty<IBrush?> colorProperty)
    {
        return new()
        {
            Children =
            {
                new KeyFrame()
                {
                    Cue = new(0),
                    Setters =
                    {
                        new Setter()
                        {
                            Property = colorProperty,
                            Value = new SolidColorBrush(fillColor),
                        }
                    },
                },
                new KeyFrame()
                {
                    Cue = new(1),
                    Setters =
                    {
                        new Setter()
                        {
                            Property = colorProperty,
                            Value = control.GetValue(colorProperty),
                        }
                    },
                },
            }
        };
    }
}
