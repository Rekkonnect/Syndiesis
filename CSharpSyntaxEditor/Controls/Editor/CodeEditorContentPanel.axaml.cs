using Avalonia;
using Avalonia.Controls;

namespace CSharpSyntaxEditor.Controls;

public partial class CodeEditorContentPanel : UserControl
{
    public static readonly StyledProperty<int> CursorLineIndexProperty =
        AvaloniaProperty.Register<CodeEditorContentPanel, int>(nameof(CursorLineIndex), defaultValue: 0);

    public int CursorLineIndex
    {
        get => GetValue(CursorLineIndexProperty);
        set
        {
            int previousLineIndex = CursorLineIndex;
            if (previousLineIndex == value)
                return;

            SetValue(CursorLineIndexProperty, value);

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
            int previousCharacterIndex = CursorCharacterIndex;
            if (previousCharacterIndex == value)
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
        int index = CursorLineIndex;
        return LineAtIndex(index);
    }

    private CodeEditorLine LineAtIndex(int index)
    {
        return (CodeEditorLine)codeLinesPanel.Children[index];
    }
}
