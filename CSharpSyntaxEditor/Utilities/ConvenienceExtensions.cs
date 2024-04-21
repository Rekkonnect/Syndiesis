using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.Styling;

namespace CSharpSyntaxEditor.Utilities;

public static class ConvenienceExtensions
{
    public static void ApplySetter(this Setter setter, AvaloniaObject control)
    {
        if (setter is DynamicSetter dynamicSetter)
        {
            dynamicSetter.Apply();
        }

        control.SetValue(setter.Property!, setter.Value);
    }

    public static ValueWithBinding GetValueWithBinding(
        this AvaloniaObject obj, AvaloniaProperty property)
    {
        var current = obj.GetValue(property);
        var subject = obj.GetBindingSubject(property);
        return new(current, subject);
    }
}
