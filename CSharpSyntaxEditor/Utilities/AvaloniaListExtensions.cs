using Avalonia.Collections;
using System.Collections.Generic;

namespace CSharpSyntaxEditor.Utilities;

public static class AvaloniaListExtensions
{
    public static void ClearSetValues<T>(this AvaloniaList<T> source, IReadOnlyList<T> values)
    {
        source.Clear();
        source.AddRange(values);
    }
}
