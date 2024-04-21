using System;

namespace CSharpSyntaxEditor.Utilities;

public static class StringManipulationExtensions
{
    public static string InsertAt(this string s, int index, char value)
    {
        if (index is 0)
            return value + s;

        if (index == s.Length)
            return s + value;

        return s[..index] + value + s[index..];
    }

    public static string RemoveBackwards(this string s, int start, int count)
    {
        int newStart = start - count + 1;
        return s.Remove(newStart, count);
    }

    public static bool IsMultiline(this string s)
    {
        bool hasLine = false;
        foreach (var _ in s.AsSpan().EnumerateLines())
        {
            if (hasLine)
                return true;
            hasLine = true;
        }
        return false;
    }
}
