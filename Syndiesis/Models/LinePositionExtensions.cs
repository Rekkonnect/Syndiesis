using Microsoft.CodeAnalysis.Text;

namespace Syndiesis.Models;

public static class LinePositionExtensions
{
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
