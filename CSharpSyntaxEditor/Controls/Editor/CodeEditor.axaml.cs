using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using CSharpSyntaxEditor.Utilities;
using System;
using System.Threading.Tasks;

namespace CSharpSyntaxEditor.Controls;

public partial class CodeEditor : UserControl
{
    // intended to remain constant for this app
    public const double LineHeight = 20;

    public static readonly StyledProperty<int> SelectedLineIndexProperty =
        AvaloniaProperty.Register<CodeEditor, int>(nameof(CursorLineIndex), defaultValue: 1);

    private MultilineStringEditor _editor = new();

    public int CursorLineIndex
    {
        get => codeEditorContent.GetValue(CodeEditorContentPanel.CursorLineIndexProperty);
        set
        {
            lineDisplayPanel.SelectedLineNumber = value + 1;
            codeEditorContent.SetValue(CodeEditorContentPanel.CursorLineIndexProperty, value);
        }
    }

    public int CursorCharacterIndex
    {
        get => codeEditorContent.CursorCharacterIndex;
        set
        {
            codeEditorContent.CursorCharacterIndex = value;
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
        UpdateVisibleText();
        CursorLineIndex = 0;
        CursorCharacterIndex = 0;
        TriggerCodeChanged();
    }

    private void UpdateVisibleTextTriggerCodeChanged()
    {
        QueueUpdateVisibleText();
        TriggerCodeChanged();
    }

    private void QueueUpdateVisibleText()
    {
        InvalidateArrange();
        _hasRequestedTextUpdate = true;
    }

    private bool _hasRequestedTextUpdate = false;

    protected override Size ArrangeOverride(Size finalSize)
    {
        ConsumeUpdateTextRequest();
        return base.ArrangeOverride(finalSize);
    }

    private void ConsumeUpdateTextRequest()
    {
        if (_hasRequestedTextUpdate)
        {
            _hasRequestedTextUpdate = false;
            UpdateVisibleText();
        }
    }

    private void UpdateVisibleText()
    {
        var linesPanel = codeEditorContent.codeLinesPanel;
        linesPanel.Children.Clear();

        int lineCount = _editor.LineCount;
        for (int i = 0; i < lineCount; i++)
        {
            var lineDisplay = new CodeEditorLine
            {
                Inlines = [new Run(_editor.AtLine(i))]
            };
            linesPanel.Children.Add(lineDisplay);
        }

        lineDisplayPanel.LineNumberStart = 1;
        lineDisplayPanel.LastLineNumber = lineCount;
        lineDisplayPanel.ForceRender();

        CursorLineIndex = lineCount - 1;
        var lastLine = _editor.AtLine(lineCount - 1);
        CursorCharacterIndex = lastLine.Length;
    }

    private void TriggerCodeChanged()
    {
        CodeChanged?.Invoke();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // adjust the min width here to ensure the line selection background fills out the displayed line
        codeEditorContent.MinWidth = availableSize.Width + 200;
        return base.MeasureOverride(availableSize);
    }

    private void GetCurrentTextPosition(out int line, out int column)
    {
        line = CursorLineIndex;
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
        CursorCharacterIndex += text.Length;
        UpdateVisibleTextTriggerCodeChanged();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        var modifiers = e.KeyModifiers;
        switch (e.Key)
        {
            case Key.Back:
                if (modifiers is KeyModifiers.Control)
                {
                    DeleteCommonCharacterGroupBackwards();
                    e.Handled = true;
                    break;
                }
                DeleteCurrentCharacterBackwards();
                e.Handled = true;
                break;

            case Key.Delete:
                if (modifiers is KeyModifiers.Control)
                {
                    DeleteCommonCharacterGroupForwards();
                    e.Handled = true;
                    break;
                }
                DeleteCurrentCharacterForwards();
                e.Handled = true;
                break;

            case Key.Enter:
                InsertLine();
                e.Handled = true;
                break;

            case Key.V:
                if (modifiers is KeyModifiers.Control)
                {
                    _ = PasteClipboardTextAsync();
                    e.Handled = true;
                }
                break;

            case Key.C:
                if (modifiers is KeyModifiers.Control)
                {
                    _ = CopySelectionToClipboardAsync();
                    e.Handled = true;
                }
                break;
        }

        base.OnKeyDown(e);
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
        UpdateVisibleTextTriggerCodeChanged();
    }

    private void InsertLine()
    {
        GetCurrentTextPosition(out int line, out int column);
        _editor.InsertLineAtColumn(line, column);
        UpdateVisibleTextTriggerCodeChanged();
    }

    private void DeleteCurrentCharacterBackwards()
    {
        GetCurrentTextPosition(out int line, out int column);
        _editor.RemoveBackwardsAt(line, column, 1);
        UpdateVisibleTextTriggerCodeChanged();
    }

    private void DeleteCurrentCharacterForwards()
    {
        GetCurrentTextPosition(out int line, out int column);
        _editor.RemoveForwardsAt(line, column, 1);
        UpdateVisibleTextTriggerCodeChanged();
    }

    private void DeleteCommonCharacterGroupBackwards()
    {
        GetCurrentTextPosition(out int line, out int column);

        if ((line, column) is (0, 0))
            return;

        if ((line, column) is ( > 0, 0))
        {
            _editor.RemoveNewLineIntoBelow(line - 1);
            UpdateVisibleTextTriggerCodeChanged();
            return;
        }

        var currentLine = _editor.AtLine(line);
        int previousColumn = column - 1;
        int start = currentLine.LeftmostContiguousCommonCategoryIndex(previousColumn);

        _editor.RemoveRangeInLine(line, start, previousColumn);
        CursorCharacterIndex = start;
        UpdateVisibleTextTriggerCodeChanged();
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
            UpdateVisibleTextTriggerCodeChanged();
            return;
        }

        int end = currentLine.RightmostContiguousCommonCategoryIndex(column);

        _editor.RemoveRangeInLine(line, column, end);
        UpdateVisibleTextTriggerCodeChanged();
    }
}
