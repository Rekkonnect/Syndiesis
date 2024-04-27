using System;

namespace Syndiesis.Utilities;

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

    public static int LeftmostContiguousCommonCategoryIndex(this string s, int index)
    {
        if (index >= s.Length)
            return -1;

        if (index < 0)
            return -1;

        var currentChar = s[index];
        var targetCategory = char.GetUnicodeCategory(currentChar);
        int leftmost = index;

        while (leftmost > 0)
        {
            int next = leftmost - 1;
            currentChar = s[next];
            var category = char.GetUnicodeCategory(currentChar);
            if (category != targetCategory)
                break;

            leftmost = next;
        }
        return leftmost;
    }

    public static int RightmostContiguousCommonCategoryIndex(this string s, int index)
    {
        if (index >= s.Length)
            return -1;

        if (index < 0)
            return -1;

        var currentChar = s[index];
        var targetCategory = char.GetUnicodeCategory(currentChar);
        int rightmost = index;

        while (rightmost < s.Length - 1)
        {
            int next = rightmost + 1;
            currentChar = s[next];
            var category = char.GetUnicodeCategory(currentChar);
            if (category != targetCategory)
                break;

            rightmost = next;
        }
        return rightmost;
    }
}
