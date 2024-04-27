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
}
