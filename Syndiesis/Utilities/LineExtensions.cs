using Microsoft.CodeAnalysis.Text;

namespace Syndiesis.Utilities;

public static class LineExtensions
{
    public static void SetLineIndex(this ref LinePosition linePosition, int line)
    {
        linePosition = new(line, linePosition.Character);
    }

    public static void SetCharacterIndex(this ref LinePosition linePosition, int character)
    {
        linePosition = new(linePosition.Line, character);
    }

    public static bool IsEmpty(this LinePositionSpan span)
    {
        return span.Start == span.End;
    }

    /// <remarks>
    /// This method was avoided as probably suboptimal compared to comparing simple integers
    /// to represent positions.
    /// </remarks>
    public static bool Contains(this LinePositionSpan span, LinePosition position)
    {
        var start = span.Start;
        var end = span.End;
        var startLine = start.Line;
        var endLine = end.Line;
        var startCharacter = start.Character;
        var endCharacter = end.Character;

        var positionLine = position.Line;
        var positionCharacter = position.Character;

        if (positionLine < startLine)
        {
            return false;
        }
        if (positionLine == startLine)
        {
            if (positionCharacter < startCharacter)
                return false;
        }

        if (positionLine > endLine)
        {
            return false;
        }
        if (positionLine == endLine)
        {
            if (positionCharacter > endCharacter)
                return false;
        }

        return true;
    }

    public static void Deconstruct(this LinePosition linePosition, out int line, out int character)
    {
        line = linePosition.Line;
        character = linePosition.Character;
    }

    public static bool IsStart(this LinePosition linePosition)
    {
        return linePosition is (0, 0);
    }

    public static bool IsFirstLine(this LinePosition linePosition)
    {
        return linePosition.Line is 0;
    }

    public static bool IsFirstCharacter(this LinePosition linePosition)
    {
        return linePosition.Character is 0;
    }

    public static LinePositionSpan FromBounds(LinePosition a, LinePosition b)
    {
        if (a < b)
        {
            return new(a, b);
        }
        else
        {
            return new(b, a);
        }
    }
}
