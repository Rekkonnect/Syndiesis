using System;
using System.Buffers;

namespace Syndiesis.Utilities;

public static class StringManipulationExtensions
{
    private static readonly SearchValues<char> _whitespaceCharacters
        = SearchValues.Create(" \t\r\n");

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

    /// <summary>
    /// Gets the leading whitespace of a string, which is the whitespace that
    /// appears at the left side of the string (assuming left-to-right / LTR content).
    /// </summary>
    /// <returns>
    /// The leading whitespace of the string. If the entire string is whitespace,
    /// the entire string is returned. If no leading whitespace exists, the empty
    /// string is returned.
    /// </returns>
    public static string GetLeadingWhitespace(this string s)
    {
        int nonWhitespace = s.AsSpan().IndexOfAnyExcept(_whitespaceCharacters);
        if (nonWhitespace < 0)
            return s;

        if (nonWhitespace is 0)
            return string.Empty;

        return s[..nonWhitespace];
    }

    /// <summary>
    /// Gets the trailing whitespace of a string, which is the whitespace that
    /// appears at the right side of the string (assuming left-to-right / LTR content).
    /// </summary>
    /// <returns>
    /// The trailing whitespace of the string. If the entire string is whitespace,
    /// the entire string is returned. If no trailing whitespace exists, the empty
    /// string is returned.
    /// </returns>
    public static string GetTrailingWhitespace(this string s)
    {
        int nonWhitespace = s.AsSpan().LastIndexOfAnyExcept(_whitespaceCharacters);
        if (nonWhitespace < 0)
            return s;

        if (nonWhitespace == s.Length - 1)
            return string.Empty;

        return s[nonWhitespace..];
    }
}
