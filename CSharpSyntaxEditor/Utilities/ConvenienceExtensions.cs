using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;

namespace CSharpSyntaxEditor.Utilities;

public static class ConvenienceExtensions
{
    public static void ApplySetter(this Setter setter, AvaloniaObject control)
    {
        control.SetValue(setter.Property!, setter.Value);
    }
}
