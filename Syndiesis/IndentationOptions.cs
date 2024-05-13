using Syndiesis.Utilities;
using System.Text.Json.Serialization;

namespace Syndiesis;

public class IndentationOptions
{
    public char IndentationCharacter = WhitespaceFacts.Space;
    public int IndentationWidth = 4;

    /// <summary>
    /// The maximum physical width that a tab may occupy.
    /// This denotes the conversion of a tab character into single space characters,
    /// and defines how many spaces should be occupying the tab fill of the column block.
    /// Each column block is also defined from this.
    /// </summary>
    public int TabWidth = 4;

    [JsonIgnore]
    public WhitespaceKind WhitespaceKind
    {
        get => IndentationCharacter.GetWhitespaceKind();
        set => IndentationCharacter = value.GetWhitespaceChar();
    }

    [JsonIgnore]
    public Indentation Indentation
    {
        get => new(IndentationCharacter, IndentationWidth);
        set
        {
            (IndentationCharacter, IndentationWidth) = value;
        }
    }
}
