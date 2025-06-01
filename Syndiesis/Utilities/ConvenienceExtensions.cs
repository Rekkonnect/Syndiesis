using Garyon.Reflection;
using System.Collections.Immutable;
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

    public static double ZeroOnNaN(this double value)
    {
        return value is double.NaN ? 0 : value;
    }

    public static bool IsEmpty<T>(this IReadOnlyList<T> source)
    {
        var genericDefinition = source.GetType().GetGenericTypeDefinitionOrSame();
        if (genericDefinition == typeof(ImmutableArray<>))
        {
            return IsEmptyImmutable(source as dynamic);
        }

        return source.Count is 0;
    }

    // This is necessary to avoid retrieving the Count property, which throws an
    // exception if the array is default. This design is highly inappropriate.
    private static bool IsEmptyImmutable<T>(ImmutableArray<T> array)
    {
        return array.IsDefaultOrEmpty;
    }

    public static void AddRange<TCollection, TElement>(
        this ICollection<TCollection> collection, IEnumerable<TElement> values)
        where TElement : TCollection
    {
        foreach (var value in values)
        {
            collection.Add(value);
        }
    }

    public static void AddOneOrMany<TCollectionElement, TAdded>(
        this ICollection<TCollectionElement> collection, OneOrMany<TAdded> oneOrMany)
        where TAdded : TCollectionElement
    {
        switch (oneOrMany.CollectionMode)
        {
            case OneOrMany<TAdded>.Mode.One:
                collection.Add(oneOrMany.Single);
                break;
            case OneOrMany<TAdded>.Mode.Many:
                collection.AddRange(oneOrMany.Enumerable);
                break;
        }
    }

    public static void SwitchInvoke(this bool value, Action whenTrue, Action whenFalse)
    {
        var action = value ? whenTrue : whenFalse;
        action();
    }
}
