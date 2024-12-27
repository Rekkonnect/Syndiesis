using System.Collections.Immutable;

namespace Syndiesis.Core;

public static class ImmutableExtensions
{
    // ImmutableArray
    public static void AddNonNull<T>(this ImmutableArray<T>.Builder builder, T? value)
        where T : class
    {
        if (value is null)
            return;

        builder.Add(value);
    }
}
