using Avalonia.Media;

namespace Syndiesis.Core;

/// <summary>
/// Represents a highlighting range within a document.
/// </summary>
/// <param name="StartLine">The inclusive zero-based start line of the range.</param>
/// <param name="StartColumn">The inclusive zero-based start column of the range.</param>
/// <param name="EndLine">The exclusive zero-based end line of the range.</param>
/// <param name="EndColumn">The exclusive zero-based end column of the range.</param>
/// <param name="Highlight">The color to highlight the span with.</param>
public record DocumentHighlightRange(int StartLine, int StartColumn, int EndLine, int EndColumn, Color Highlight)
{
    public bool ContainsEntireStartLine
    {
        get
        {
            return StartColumn is 0
                && EndLine > StartLine;
        }
    }

    public bool ContainsEntireLine(int line)
    {
        if (line == StartLine)
        {
            if (ContainsEntireStartLine)
            {
                return true;
            }
        }

        return StartLine < line && line < EndLine;
    }
}
