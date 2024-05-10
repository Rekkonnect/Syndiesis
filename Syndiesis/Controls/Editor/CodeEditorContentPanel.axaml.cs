using Avalonia.Controls;
using Syndiesis.Utilities;

namespace Syndiesis.Controls;

public partial class CodeEditorContentPanel : UserControl
{
    public CodeEditorContentPanel()
    {
        InitializeComponent();
    }

    private CodeEditorLine? LineAtIndex(int index)
    {
        return codeLinesPanel.Children.ValueAtOrDefault(index) as CodeEditorLine;
    }
}
