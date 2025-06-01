using AvaloniaEdit.Rendering;

namespace Syndiesis.Controls.Editor;

public partial class BackgroundLineNumberPanel : UserControl
{
    public BackgroundLineNumberPanel(TextView view)
    {
        InitializeComponent();
        lines.TextView = view;
    }
}
