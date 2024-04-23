using Avalonia;
using Avalonia.Controls;

namespace CSharpSyntaxEditor.Controls;

public partial class CodeEditorContentPanel : UserControl
{
    public static readonly StyledProperty<int> SelectedLineIndexProperty =
        AvaloniaProperty.Register<CodeEditorContentPanel, int>(nameof(SelectedLineIndex), defaultValue: 0);

    public int SelectedLineIndex
    {
        get => GetValue(SelectedLineIndexProperty);
        set
        {
            int previousLineIndex = SelectedLineIndex;
            if (previousLineIndex == value)
                return;

            SetValue(SelectedLineIndexProperty, value);

            var previousLine = LineAtIndex(previousLineIndex);
            var currentLine = LineAtIndex(value);
            previousLine.SelectedLine = false;
            currentLine.SelectedLine = true;
        }
    }

    public static readonly StyledProperty<int> CursorCharacterIndexProperty =
        AvaloniaProperty.Register<CodeEditorContentPanel, int>(nameof(CursorCharacterIndex), defaultValue: 0);

    public int CursorCharacterIndex
    {
        get
        {
            var selectedLine = CurrentlySelectedLine();
            return selectedLine.GetValue(CodeEditorLine.CursorCharacterIndexProperty);
        }
        set
        {
            int previousLineIndex = SelectedLineIndex;
            if (previousLineIndex == value)
                return;

            var selectedLine = CurrentlySelectedLine();
            selectedLine.CursorCharacterIndex = value;
        }
    }

    public CodeEditorContentPanel()
    {
        InitializeComponent();
    }

    public CodeEditorLine CurrentlySelectedLine()
    {
        int index = SelectedLineIndex;
        return LineAtIndex(index);
    }

    private CodeEditorLine LineAtIndex(int index)
    {
        return (CodeEditorLine)codeLinesPanel.Children[index];
    }
}
