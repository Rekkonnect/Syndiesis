using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using CSharpSyntaxEditor.Utilities;
using System;
using System.Threading.Tasks;

namespace CSharpSyntaxEditor.Controls;

public partial class CodeEditor : UserControl
{
    // intended to remain constant for this app
    public const int LineHeight = 20;

    public static readonly StyledProperty<int> SelectedLineIndexProperty =
        AvaloniaProperty.Register<CodeEditor, int>(nameof(SelectedLineIndex), defaultValue: 1);

    private MultilineStringEditor _editor = new();

    public int SelectedLineIndex
    {
        get => codeEditorContent.GetValue(CodeEditorContentPanel.SelectedLineIndexProperty);
        set
        {
            codeEditorContent.SetValue(CodeEditorContentPanel.SelectedLineIndexProperty, value);
            RestartCursorAnimation();
        }
    }

    public int CursorCharacterIndex
    {
        get => codeEditorContent.GetValue(CodeEditorContentPanel.CursorCharacterIndexProperty);
        set
        {
            codeEditorContent.SetValue(CodeEditorContentPanel.CursorCharacterIndexProperty, value);
            RestartCursorAnimation();
        }
    }

    public MultilineStringEditor Editor
    {
        get => _editor;
        set
        {
            _editor = value;
            TriggerCodeChanged();
        }
    }

    public event Action? CodeChanged;

    public CodeEditor()
    {
        InitializeComponent();
        Focusable = true;
    }

    public void SetSource(string source)
    {
        _editor.SetText(source);
        TriggerCodeChanged();
    }

    private void TriggerCodeChanged()
    {
        CodeChanged?.Invoke();
    }

    private void RestartCursorAnimation()
    {
        codeEditorContent.CurrentlySelectedLine().RestartCursorAnimation();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // adjust the min width here to ensure the line selection background fills out the displayed line
        codeEditorContent.MinWidth = availableSize.Width + 200;
        return base.MeasureOverride(availableSize);
    }

    private void GetCurrentTextPosition(out int line, out int column)
    {
        line = SelectedLineIndex;
        column = CursorCharacterIndex;
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);
        var text = e.Text;
        if (text is null)
            return;

        GetCurrentTextPosition(out int line, out int column);
        _editor.InsertAt(line, column, text);
        TriggerCodeChanged();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        var modifiers = e.KeyModifiers;
        switch (e.Key)
        {
            case Key.Back:
                if (modifiers is KeyModifiers.Control)
                {
                    DeleteCommonCharacterGroupBackwards();
                    break;
                }
                DeleteCurrentCharacterBackwards();
                break;

            case Key.Delete:
                if (modifiers is KeyModifiers.Control)
                {
                    DeleteCommonCharacterGroupForwards();
                    break;
                }
                DeleteCurrentCharacterForwards();
                break;

            case Key.Enter:
                InsertLine();
                break;

            case Key.V:
                if (modifiers is KeyModifiers.Control)
                {
                    _ = PasteClipboardTextAsync();
                }
                break;

            case Key.C:
                if (modifiers is KeyModifiers.Control)
                {
                    _ = CopySelectionToClipboardAsync();
                }
                break;
        }
    }

    private async Task CopySelectionToClipboardAsync()
    {
        // TODO: Handle selection
        var selection = string.Empty;

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is null)
            return;

        await clipboard.SetTextAsync(selection);
    }

    private async Task PasteClipboardTextAsync()
    {
        // Get the positions before we paste the text
        // in which case the user might have buffered inputs that we
        // display before getting the content to paste,
        // therefore pasting the content on the position at which the
        // paste command was input
        // Will only break user input if the user inserts new input before
        // the paste location, which is rarer
        GetCurrentTextPosition(out int line, out int column);

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is null)
            return;

        var pasteText = await clipboard.GetTextAsync();
        if (pasteText is null)
            return;

        // TODO: Handle selection
        _editor.InsertAt(line, column, pasteText);
    }

    private void InsertLine()
    {
        GetCurrentTextPosition(out int line, out int column);
        _editor.InsertLineAtColumn(line, column);
    }

    private void DeleteCurrentCharacterBackwards()
    {
        GetCurrentTextPosition(out int line, out int column);
        _editor.RemoveBackwardsAt(line, column, 1);
    }

    private void DeleteCurrentCharacterForwards()
    {
        GetCurrentTextPosition(out int line, out int column);
        _editor.RemoveForwardsAt(line, column, 1);
    }

    private void DeleteCommonCharacterGroupBackwards()
    {
        GetCurrentTextPosition(out int line, out int column);

        if ((line, column) is (0, 0))
            return;

        if ((line, column) is (> 0, 0))
        {
            _editor.RemoveNewLineIntoBelow(line - 1);
            return;
        }

        var currentLine = _editor.AtLine(line);
        int previousColumn = column - 1;
        int start = currentLine.LeftmostContiguousCommonCategoryIndex(previousColumn);

        _editor.RemoveRangeInLine(line, start, previousColumn);
        CursorCharacterIndex = start;
    }

    private void DeleteCommonCharacterGroupForwards()
    {
        GetCurrentTextPosition(out int line, out int column);

        if (line >= _editor.LineCount)
            return;

        var currentLine = _editor.AtLine(line);

        if (column >= currentLine.Length - 1)
        {
            if (line == _editor.LineCount - 1)
            {
                return;
            }

            _editor.RemoveNewLineIntoBelow(line);
            return;
        }

        int end = currentLine.RightmostContiguousCommonCategoryIndex(column);

        _editor.RemoveRangeInLine(line, column, end);
    }
}
