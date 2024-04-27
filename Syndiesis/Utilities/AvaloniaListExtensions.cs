using Avalonia.Collections;
using System.Collections.Generic;

namespace Syndiesis.Utilities;

public static class AvaloniaListExtensions
{
    public static void ClearSetValues<T>(this AvaloniaList<T> source, IReadOnlyList<T> values)
    {
        source.Clear();
        source.AddRange(values);
    }
}
