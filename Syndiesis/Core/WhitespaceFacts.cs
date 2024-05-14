using System;

namespace Syndiesis.Core;

public static class WhitespaceFacts
{
    public const char Space = ' ';
    public const char Tab = '\t';

    public static WhitespaceKind GetWhitespaceKind(this char c)
    {
        return c switch
        {
            Space => WhitespaceKind.Space,
            Tab => WhitespaceKind.Tab,
            '\r' or
            '\n' => WhitespaceKind.EndOfLine,
            _ => WhitespaceKind.None,
        };
    }

    public static char GetWhitespaceChar(this WhitespaceKind kind)
    {
        return kind switch
        {
            WhitespaceKind.Space => Space,
            WhitespaceKind.Tab => '\t',
            WhitespaceKind.EndOfLine => throw new ArgumentException("Cannot get single possible end of line whitespace character"),
            _ => throw new ArgumentException("Invalid whitespace kind"),
        };
    }
}
