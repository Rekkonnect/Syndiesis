namespace Syndiesis.Core;

public static class CollectionExtensions
{
    public static void AddNonNull<T>(this ICollection<T> collection, T? value)
        where T : class
    {
        if (value is null)
            return;

        collection.Add(value);
    }
}
