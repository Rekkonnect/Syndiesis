using System;
using System.Collections.Generic;
using System.Numerics;

namespace Syndiesis.Utilities;

public static class ConvenienceExtensions
{
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

    public static int RoundInt32(this double value)
    {
        return (int)Math.Round(value);
    }
}
