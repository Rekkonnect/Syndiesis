using AvaloniaEdit;

namespace Syndiesis.Controls;

public sealed class SyndiesisTextEditor : TextEditor
{
    public new SyndiesisTextArea TextArea => (SyndiesisTextArea)base.TextArea;

    public SyndiesisTextEditor()
        : base(new SyndiesisTextArea())
    {
    }
}
