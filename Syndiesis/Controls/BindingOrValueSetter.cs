using Avalonia;
using Avalonia.Styling;
using System;

namespace Syndiesis.Controls;

public sealed class BindingOrValueSetter : Setter
{
    public BindingOrValueSetter(AvaloniaProperty property, object? value)
        : base(property, GetValue(value))
    {
        if (value is ValueWithBinding valueBinding)
        {
            HandleSubject(valueBinding);
        }
    }

    private void HandleSubject(ValueWithBinding valueBinding)
    {
        var subject = valueBinding.BindingSubject;
        subject.Subscribe(newValue =>
        {
            Value = newValue.Value;
        });
    }

    private static object? GetValue(object? value)
    {
        if (value is ValueWithBinding valueBinding)
        {
            return valueBinding.CurrentValue;
        }
        return value;
    }
}
