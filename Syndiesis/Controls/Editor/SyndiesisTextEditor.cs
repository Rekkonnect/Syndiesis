using AvaloniaEdit;

namespace Syndiesis.Controls;

public sealed class SyndiesisTextEditor : TextEditor
{
    public SyndiesisTextEditor()
        : base(new SyndiesisTextArea())
    {
    }
}
