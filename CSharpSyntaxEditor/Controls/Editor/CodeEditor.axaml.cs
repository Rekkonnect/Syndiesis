using Avalonia;
using Avalonia.Controls;

namespace CSharpSyntaxEditor.Controls;

public partial class CodeEditor : UserControl
{
    // intended to remain constant for this demonstration
    public const int LineHeight = 20;

    public static readonly StyledProperty<int> SelectedLineIndexProperty =
        AvaloniaProperty.Register<CodeEditor, int>(nameof(SelectedLineIndex), defaultValue: 1);

    public int SelectedLineIndex
    {
        get => codeEditorContent.GetValue(CodeEditorContentPanel.SelectedLineIndexProperty);
        set => codeEditorContent.SetValue(CodeEditorContentPanel.SelectedLineIndexProperty, value);
    }

    public CodeEditor()
    {
        InitializeComponent();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // adjust the min width here to ensure the line selection background fills out the displayed line
        codeEditorContent.MinWidth = availableSize.Width + 200;
        return base.MeasureOverride(availableSize);
    }
}
