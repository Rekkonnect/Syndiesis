using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using CSharpSyntaxEditor.Models;
using CSharpSyntaxEditor.Utilities;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace CSharpSyntaxEditor.Controls;

public partial class CodeEditor : UserControl
{
    // intended to remain constant for this app
    public const double LineHeight = 20;

    public static readonly StyledProperty<int> SelectedLineIndexProperty =
        AvaloniaProperty.Register<CodeEditor, int>(nameof(CursorLineIndex), defaultValue: 1);

    private MultilineStringEditor _editor = new();
    private readonly CodeEditorLineBuffer _lineBuffer = new();

    private LinePosition _cursorLinePosition;
    private readonly SelectionSpan _selectionSpan = new();

    private int _preferredCursorCharacterIndex;

    public int CursorLineIndex
    {
        get => _cursorLinePosition.Line;
        set
        {
            _cursorLinePosition.SetLineIndex(value);
            lineDisplayPanel.SelectedLineNumber = value + 1;
            codeEditorContent.CursorLineIndex = value;
            codeEditorContent.CurrentlySelectedLine().RestartCursorAnimation();
        }
    }

    public int CursorCharacterIndex
    {
        get => _cursorLinePosition.Character;
        set
        {
            _cursorLinePosition.SetCharacterIndex(value);
            codeEditorContent.CursorCharacterIndex = value;
            codeEditorContent.CurrentlySelectedLine().RestartCursorAnimation();
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
        int totalVisible = 40;
        _lineBuffer.SetCapacity(totalVisible);
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

        int lineStart = 0;
        _lineBuffer.LoadFrom(lineStart, _editor);
        int lineCount = _editor.LineCount;
        const int visibleLines = 40;
        var lineRange = _lineBuffer.LineSpanForRange(lineStart, visibleLines);
        for (int i = 0; i < visibleLines; i++)
        {
            var lineDisplay = lineRange[i];
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
        line = _cursorLinePosition.Line;
        column = _cursorLinePosition.Character;
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

            case Key.Left:
                if (modifiers is KeyModifiers.Control)
                {
                    MoveCursorLeftWord();
                    e.Handled = true;
                    break;
                }
                MoveCursorLeft();
                e.Handled = true;
                break;

            case Key.Right:
                if (modifiers is KeyModifiers.Control)
                {
                    MoveCursorNextWord();
                    e.Handled = true;
                    break;
                }
                MoveCursorRight();
                e.Handled = true;
                break;

            case Key.Up:
                MoveCursorUp();
                e.Handled = true;
                break;

            case Key.Down:
                MoveCursorDown();
                e.Handled = true;
                break;

            case Key.PageUp:
                if (modifiers is KeyModifiers.Control)
                {
                    e.Handled = true;
                    break;
                }
                MoveCursorRight();
                e.Handled = true;
                break;

            case Key.PageDown:
                if (modifiers is KeyModifiers.Control)
                {
                    e.Handled = true;
                    break;
                }
                MoveCursorRight();
                e.Handled = true;
                break;

            case Key.Home:
                if (modifiers is KeyModifiers.Control)
                {
                    MoveCursorDocumentStart();
                    e.Handled = true;
                    break;
                }
                MoveCursorLineStart();
                e.Handled = true;
                break;

            case Key.End:
                if (modifiers is KeyModifiers.Control)
                {
                    MoveCursorDocumentEnd();
                    e.Handled = true;
                    break;
                }
                MoveCursorLineEnd();
                e.Handled = true;
                break;
        }

        base.OnKeyDown(e);
    }

    private void MoveCursorLeftWord()
    {
        var position = LeftmostContiguousCommonCategory();
        SetCursorPosition(position);
        CapturePreferredCursorCharacter();
    }

    private void MoveCursorNextWord()
    {
        var position = RightmostContiguousCommonCategory();
        SetCursorPosition(position);
        CapturePreferredCursorCharacter();
    }

    private void MoveCursorLineStart()
    {
        CursorCharacterIndex = 0;
        CapturePreferredCursorCharacter();
    }

    private void MoveCursorLineEnd()
    {
        CursorCharacterIndex = _editor.LineLength(_cursorLinePosition.Line);
        CapturePreferredCursorCharacter();
    }

    private void MoveCursorDocumentStart()
    {
        SetCursorPosition(new(0, 0));
        CapturePreferredCursorCharacter();
    }

    private void MoveCursorDocumentEnd()
    {
        CursorLineIndex = _editor.LineCount - 1;
        int lineLength = _editor.LineLength(_cursorLinePosition.Line);
        CursorCharacterIndex = lineLength;
        CapturePreferredCursorCharacter();
    }

    private void MoveCursorDown()
    {
        GetCurrentTextPosition(out var line, out var column);
        int lineCount = _editor.LineCount;
        if (line >= lineCount - 1)
            return;

        CursorLineIndex++;
        PlaceCursorCharacterIndexAfterVerticalMovement(column);
    }

    private void MoveCursorUp()
    {
        GetCurrentTextPosition(out var line, out var column);
        if (line is 0)
            return;

        CursorLineIndex--;
        PlaceCursorCharacterIndexAfterVerticalMovement(column);
    }

    private void PlaceCursorCharacterIndexAfterVerticalMovement(int column)
    {
        var lineLength = _editor.LineLength(CursorLineIndex);
        if (column > lineLength)
        {
            CursorCharacterIndex = lineLength;
        }
        else
        {
            CursorCharacterIndex = Math.Min(_preferredCursorCharacterIndex, lineLength);
        }
    }

    private void MoveCursorLeft()
    {
        if (_cursorLinePosition.IsFirstCharacter())
            return;

        GetCurrentTextPosition(out var line, out var column);
        if (column is 0)
        {
            CursorLineIndex = line - 1;
            CursorCharacterIndex = _editor.AtLine(line - 1).Length;
            CapturePreferredCursorCharacter();
            return;
        }

        CursorCharacterIndex--;
        CapturePreferredCursorCharacter();
    }

    private void MoveCursorRight()
    {
        GetCurrentTextPosition(out var line, out var column);
        var lineContent = _editor.AtLine(line);
        int lineLength = lineContent.Length;
        if (column == lineLength)
        {
            if (line == _editor.LineCount - 1)
            {
                // no more text to go to
                return;
            }

            CursorLineIndex = line + 1;
            CursorCharacterIndex = 0;
            CapturePreferredCursorCharacter();
            return;
        }

        CursorCharacterIndex++;
        CapturePreferredCursorCharacter();
    }

    private void SetCursorPosition(LinePosition linePosition)
    {
        (CursorLineIndex, CursorCharacterIndex) = linePosition;
    }

    private void CapturePreferredCursorCharacter()
    {
        _preferredCursorCharacterIndex = CursorCharacterIndex;
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
        CapturePreferredCursorCharacter();
        UpdateVisibleTextTriggerCodeChanged();
    }

    private void DeleteCurrentCharacterBackwards()
    {
        GetCurrentTextPosition(out int line, out int column);
        _editor.RemoveBackwardsAt(line, column, 1);
        CapturePreferredCursorCharacter();
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
        if (_cursorLinePosition.IsFirstCharacter())
            return;

        GetCurrentTextPosition(out int line, out int column);
        if ((line, column) is ( > 0, 0))
        {
            _editor.RemoveNewLineIntoBelow(line - 1);
            UpdateVisibleTextTriggerCodeChanged();
            return;
        }

        int previousColumn = column - 1;
        var start = LeftmostContiguousCommonCategory().Character;

        _editor.RemoveRangeInLine(line, start, previousColumn);
        CursorCharacterIndex = start;
        CapturePreferredCursorCharacter();
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

        var end = RightmostContiguousCommonCategory().Character;

        _editor.RemoveRangeInLine(line, column, end);
        UpdateVisibleTextTriggerCodeChanged();
    }

    private LinePosition LeftmostContiguousCommonCategory()
    {
        // we assume that the caller has sanitized the positions

        var (line, column) = _cursorLinePosition;
        Debug.Assert(line >= 0 && line < _editor.LineCount);
        var lineLength = _editor.LineLength(line);

        Debug.Assert(column >= 0 && column <= lineLength);

        bool hasConsumedWhitespace = false;
        if (column is 0)
        {
            if (line is 0)
            {
                return new(0, 0);
            }

            line--;
            column = _editor.LineLength(line);
            hasConsumedWhitespace = true;
        }

        int leftmost = column - 1;
        var lineContent = _editor.AtLine(line);

        if (lineContent.Length is 0)
            return new(line, 0);

        var targetCategory = TextEditorCharacterCategory.Whitespace;
        var firstCharacter = lineContent[leftmost];
        var previousCategory = EditorCategory(firstCharacter);

        while (leftmost >= 0)
        {
            var c = lineContent[leftmost];
            var category = EditorCategory(c);
            if (category is TextEditorCharacterCategory.Whitespace)
            {
                if (previousCategory is not TextEditorCharacterCategory.Whitespace)
                {
                    if (hasConsumedWhitespace)
                        break;
                }

                hasConsumedWhitespace = true;
            }

            // try to determine what char category we are seeking for
            if (targetCategory is TextEditorCharacterCategory.Whitespace)
            {
                targetCategory = category;
            }
            else
            {
                if (category != targetCategory)
                    break;
            }

            previousCategory = category;
            leftmost--;
        }

        return new(line, leftmost + 1);
    }

    // copy-pasting this, although ugly, seems to be better in terms of
    // guaranteeing flexibility and maintainability in this particular case
    private LinePosition RightmostContiguousCommonCategory()
    {
        // we assume that the caller has sanitized the positions

        var (line, column) = _cursorLinePosition;
        Debug.Assert(line >= 0 && line < _editor.LineCount);
        var lineLength = _editor.LineLength(line);

        Debug.Assert(column >= 0 && column <= lineLength);

        bool hasConsumedWhitespace = false;
        if (column == lineLength)
        {
            if (line == _editor.LineCount - 1)
            {
                return _cursorLinePosition;
            }

            line++;
            column = 0;
            hasConsumedWhitespace = true;
        }

        int rightmost = column;
        var lineContent = _editor.AtLine(line);

        if (lineContent.Length is 0)
            return new(line, 0);

        var targetCategory = TextEditorCharacterCategory.Whitespace;
        var firstCharacter = lineContent[rightmost];
        var previousCategory = EditorCategory(firstCharacter);

        while (rightmost < lineContent.Length)
        {
            var c = lineContent[rightmost];
            var category = EditorCategory(c);
            if (category is TextEditorCharacterCategory.Whitespace)
            {
                if (previousCategory is not TextEditorCharacterCategory.Whitespace)
                {
                    if (hasConsumedWhitespace)
                        break;
                }

                hasConsumedWhitespace = true;
            }

            // try to determine what char category we are seeking for
            if (targetCategory is TextEditorCharacterCategory.Whitespace)
            {
                targetCategory = category;
            }
            else
            {
                if (category != targetCategory)
                    break;
            }

            previousCategory = category;
            rightmost++;
        }

        return new(line, rightmost);
    }

    private static TextEditorCharacterCategory EditorCategory(char c)
    {
        if (c is '_')
            return TextEditorCharacterCategory.Identifier;

        var unicode = char.GetUnicodeCategory(c);
        switch (unicode)
        {
            case UnicodeCategory.DecimalDigitNumber:
            case UnicodeCategory.LowercaseLetter:
            case UnicodeCategory.UppercaseLetter:
                return TextEditorCharacterCategory.Identifier;

            case UnicodeCategory.SpaceSeparator:
                return TextEditorCharacterCategory.Whitespace;
        }

        return TextEditorCharacterCategory.General;
    }
}

public enum TextEditorCharacterCategory
{
    General,
    Identifier,
    Whitespace,
}
