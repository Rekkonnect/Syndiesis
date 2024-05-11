using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using Garyon.Functions;
using System.Collections.Generic;
using System.Numerics;

namespace Syndiesis.Utilities;

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

    public static T? ValueAtOrDefault<T>(this IReadOnlyList<T> list, int index)
    {
        if (index < 0)
            return default;

        if (index >= list.Count)
            return default;

        return list[index];
    }

    public static bool ValidIndex(this int value, int length)
    {
        return value >= 0
            && value < length;
    }

    public static bool ValidIndex<T>(this int value, T[] array)
    {
        return ValidIndex(value, array.Length);
    }

    public static bool ValidIndex<T>(this int value, ICollection<T> collection)
    {
        return ValidIndex(value, collection.Count);
    }

    public static void Sort<T>(T a, T b, out T min, out T max)
        where T : IComparisonOperators<T, T, bool>
    {
        if (a < b)
        {
            min = a;
            max = b;
        }
        else
        {
            min = b;
            max = a;
        }
    }
}
