using Avalonia;
using Avalonia.Styling;
using System;

namespace CSharpSyntaxEditor.Utilities;

public sealed class DynamicSetter : Setter
{
    private readonly Func<object?>? _getter;

    public DynamicSetter(AvaloniaProperty property, object? value)
        : base(property, GetValue(value))
    {
        if (value is Func<object?> getter)
        {
            _getter = getter;
        }
    }

    public void Apply()
    {
        if (_getter is not null)
        {
            Value = _getter();
        }
    }

    private static object? GetValue(object? value)
    {
        if (value is Func<object?> getter)
        {
            return getter();
        }

        return value;
    }
}
