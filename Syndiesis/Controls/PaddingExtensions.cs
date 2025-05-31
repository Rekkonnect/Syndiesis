using Avalonia;
using Avalonia.Controls;

namespace Syndiesis.Controls;

public static class PaddingExtensions
{
    public static Thickness? GetPadding(this Control control)
    {
        return control switch
        {
            Button button => button.Padding,
            TextBlock text => text.Padding,
            Decorator decorator => decorator.Padding,
            ContentControl content => content.Padding,
            _ => null,
        };
    }

    public static void SetPadding(this Control control, Thickness value)
    {
        _ = control switch
        {
            Button button => button.Padding = value,
            TextBlock text => text.Padding = value,
            Decorator decorator => decorator.Padding = value,
            ContentControl content => content.Padding = value,
            _ => default,
        };
    }
}
