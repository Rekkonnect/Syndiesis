using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.CodeAnalysis.Text;
using ReactiveUI;
using Syndiesis.Models;
using Syndiesis.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static Syndiesis.Controls.CodeEditorLine;

namespace Syndiesis.Controls;

// TODO: Migrate most of this class' cursor editing logic into another class

/// <summary>
/// A code editor supporting common code editing operations including navigating the cursor
/// through with common keyboard shortcuts, deleting characters or words, selecting a
/// range of text, inserting new lines, copying and pasting text.
/// </summary>
/// <remarks>
/// It is not meant to provide support for autocompletion, indentation preferences,
/// multiple carets, etc. Those features are outside of the scope of this program.
/// Highlighting is not yet implemented but is in the works.
/// </remarks>
public partial class CodeEditor : UserControl
{
    // intended to remain constant for this app
    public const double LineHeight = 19;
    public const double CharWidth = 8.6;

    private const double extraDisplayWidth = 200;
    private const double scrollThresholdWidth = extraDisplayWidth / 2;
    private const int visibleLinesThreshold = 4;

    private MultilineStringEditor _editor = new();
    private readonly CodeEditorLineBuffer _lineBuffer = new(20);

    private LinePosition _cursorLinePosition;
    private readonly SelectionSpan _selectionSpan = new();

    private int _extraBufferLines = 0;

    private int _lineOffset;
    private int _preferredCursorCharacterIndex;

    private bool _hasActiveHover;

    private SyntaxTreeListNode? _hoveredListNode;

    [Obsolete("This setting is not saving any performance")]
    public int ExtraBufferLines
    {
        get => _extraBufferLines;
        set
        {
            _extraBufferLines = value;
            // TODO: More?
        }
    }

    public int LineOffset
    {
        get => _lineOffset;
        set
        {
            if (_lineOffset == value)
                return;

            _lineOffset = value;
            UpdateVisibleText();
        }
    }

    public int CursorLineIndex
    {
        get => _cursorLinePosition.Line;
        set
        {
            var previousLineIndex = _cursorLinePosition.Line;
            var previousLine = _lineBuffer.GetLine(previousLineIndex);
            if (previousLine is not null)
            {
                previousLine.SelectedLine = false;
            }
            _cursorLinePosition.SetLineIndex(value);
            BringVerticalIntoView();
            lineDisplayPanel.SelectedLineNumber = value + 1;
            var nextLine = _lineBuffer.GetLine(value);
            Debug.Assert(
                nextLine is not null,
                "we have brought the line into view, so the line buffer should have loaded the line");
            nextLine.SelectedLine = true;
            nextLine.RestartCursorAnimation();
            TriggerCursorPositionChanged();
        }
    }

    public int CursorCharacterIndex
    {
        get => _cursorLinePosition.Character;
        set
        {
            _cursorLinePosition.SetCharacterIndex(value);
            var lineIndex = _cursorLinePosition.Line;
            var line = _lineBuffer.GetLine(lineIndex);
            Debug.Assert(
                line is not null,
                "we have brought the line into view, so the line buffer should have loaded the line");
            line.CursorCharacterIndex = value;
            line.RestartCursorAnimation();
            BringHorizontalIntoView();
            TriggerCursorPositionChanged();
        }
    }

    public LinePosition CursorPosition
    {
        get => _cursorLinePosition;
        set
        {
            var (line, character) = value;
            CursorLineIndex = line;
            CursorCharacterIndex = character;
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
    public event Action<LinePosition>? CursorPositionChanged;

    public CodeEditor()
    {
        InitializeComponent();
        InitializeEvents();
        Focusable = true;
    }

    private void InitializeEvents()
    {
        verticalScrollBar.ScrollChanged += OnVerticalScroll;
        horizontalScrollBar.ScrollChanged += OnHorizontalScroll;
    }

    private bool _isUpdatingScrollLimits = false;

    private void OnVerticalScroll()
    {
        if (_isUpdatingScrollLimits)
            return;

        var newLineOffset = (int)verticalScrollBar.StartPosition;
        LineOffset = newLineOffset;
        UpdateVisibleCursor();
    }

    private void UpdateVisibleCursor()
    {
        var cursorLine = CursorLineIndex;

        var lineOffset = _lineBuffer.LineOffset;
        Debug.Assert(_lineOffset == lineOffset, """
            while the line buffer has an independent line offset, it is
            possible to encounter a future change that affects this
            """);

        for (int i = 0; i < _lineBuffer.Capacity; i++)
        {
            var currentIndex = lineOffset + i;
            bool visible = cursorLine == currentIndex;
            var line = _lineBuffer.GetLine(currentIndex);
            if (line is null)
            {
                continue;
            }

            if (visible)
            {
                line.ShowCursor();
                line.CursorCharacterIndex = CursorCharacterIndex;
            }
            else
            {
                line.HideCursor();
            }
        }
    }

    private void OnHorizontalScroll()
    {
        if (_isUpdatingScrollLimits)
            return;

        var offset = horizontalScrollBar.StartPosition;
        Canvas.SetLeft(codeEditorContent, -offset);
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
        int totalVisible = VisibleLines(finalSize);
        int bufferCapacity = totalVisible + _extraBufferLines * 2;
        _lineBuffer.SetCapacity(bufferCapacity);
        return base.ArrangeOverride(finalSize);
    }

    private int VisibleLines()
    {
        return VisibleLines(codeCanvasContainer.Bounds.Size);
    }

    public static int VisibleLines(Size size)
    {
        return VisibleLines(size.Height);
    }

    public static int VisibleLines(double height)
    {
        return (int)(height / LineHeight);
    }

    private void ForceUpdateText()
    {
        _hasRequestedTextUpdate = false;
        UpdateVisibleText();
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

        int upperBoundLineStart = _lineOffset - _extraBufferLines;
        int extraBufferLines = _extraBufferLines;
        if (upperBoundLineStart < 0)
        {
            extraBufferLines += upperBoundLineStart;
        }
        int lineStart = Math.Max(0, upperBoundLineStart);
        _lineBuffer.LoadFrom(lineStart, _editor);
        int lineCount = _editor.LineCount;
        int visibleLines = VisibleLines();
        var lineRange = _lineBuffer.LineSpanForAbsoluteIndexRange(extraBufferLines, visibleLines);
        linesPanel.Children.AddRange(lineRange);
        UpdateLinesDisplayPanel(lineCount);

        ShowSelectedLine();
        ShowCurrentHoveredSyntaxNode();
        ShowCurrentTextSelection();
        UpdateEntireScroll();
    }

    private void UpdateLinesDisplayPanel()
    {
        int lineCount = VisibleLines();
        UpdateLinesDisplayPanel(lineCount);
    }

    private void UpdateLinesDisplayPanel(int lineCount)
    {
        lineDisplayPanel.LineNumberStart = _lineOffset + 1;
        int maxLines = _editor.LineCount;
        lineDisplayPanel.LastLineNumber = Math.Min(_lineOffset + lineCount, maxLines);
        lineDisplayPanel.ForceRender();
    }

    private void ShowSelectedLine()
    {
        int index = CursorLineIndex;
        var line = _lineBuffer.GetLine(index);
        if (line is null)
            return;

        line.SelectedLine = true;
        line.RestartCursorAnimation();
    }

    private void TriggerCursorPositionChanged()
    {
        if (CursorPositionChanged is null)
            return;

        CursorPositionChanged.Invoke(_cursorLinePosition);
    }

    private void TriggerCodeChanged()
    {
        CodeChanged?.Invoke();
    }

    public void ShowHoveredSyntaxNode(SyntaxTreeListNode? listNode)
    {
        _hoveredListNode = listNode;
        ShowCurrentHoveredSyntaxNode();
    }

    private void ShowCurrentHoveredSyntaxNode()
    {
        HideAllHoveredSyntaxNodes();

        if (_hoveredListNode is not null)
        {
            var syntaxObject = _hoveredListNode.AssociatedSyntaxObject;

            if (syntaxObject is not null)
            {
                var span = _hoveredListNode.NodeLine.DisplayLineSpan;
                SetHoverSpan(span, HighlightKind.SyntaxNodeHover);
            }
        }
    }

    private void ShowCurrentTextSelection()
    {
        HideAllTextSelection();

        if (_selectionSpan.HasSelection)
        {
            var span = _selectionSpan.SelectionPositionSpan;
            SetHoverSpan(span, HighlightKind.Selection);
        }
    }

    private IReadOnlyList<CodeEditorLine> CodeEditorLines()
    {
        return codeEditorContent.codeLinesPanel.Children
            .UpcastReadOnlyList<Control, CodeEditorLine>();
    }

    private void HideAllHoveredSyntaxNodes()
    {
        if (!_hasActiveHover)
            return;

        ClearAllHighlights(HighlightKind.SyntaxNodeHover);
        _hasActiveHover = false;
    }

    private void HideAllTextSelection()
    {
        if (!_selectionSpan.HasSelection)
            return;

        ClearAllHighlights(HighlightKind.Selection);
    }

    private void ClearAllHighlights(HighlightKind syntaxNodeHover)
    {
        foreach (var line in CodeEditorLines())
        {
            line.GetHighlightHandler(syntaxNodeHover).Clear();
        }
    }

    private void SetHoverSpan(LinePositionSpan span, HighlightKind highlightKind)
    {
        var start = span.Start;
        var end = span.End;
        var startLine = start.Line;
        var endLine = end.Line;

        var lines = CodeEditorLines();

        if (startLine == endLine)
        {
            var startEditorLine = EditorLineAt(startLine);
            var startCharacter = start.Character;
            var endCharacter = end.Character;
            startEditorLine?.GetHighlightHandler(highlightKind)
                .Set(startCharacter..endCharacter);
        }
        else
        {
            for (int i = startLine + 1; i < endLine; i++)
            {
                var editorLine = EditorLineAt(i);
                editorLine?.GetHighlightHandler(highlightKind)
                    .SetEntireLine(editorLine, true);
            }

            var startEditorLine = EditorLineAt(startLine);
            startEditorLine?.GetHighlightHandler(highlightKind)
                .SetRightPart(start.Character, startEditorLine, true);

            var endEditorLine = EditorLineAt(endLine);
            endEditorLine?.GetHighlightHandler(highlightKind)
                .SetLeftPart(end.Character);
        }

        _hasActiveHover = true;

        CodeEditorLine? EditorLineAt(int i)
        {
            int lineIndex = i - _lineOffset;
            var editorLine = lines.ValueAtOrDefault(lineIndex);
            return editorLine;
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var canvasOffset = e.GetPosition(codeCanvas);
        bool contained = codeCanvas.Bounds.Contains(canvasOffset);
        if (!contained)
            return;

        int pointerLine = (int)(canvasOffset.Y / LineHeight);
        int pointerColumn = (int)((canvasOffset.X + GetHorizontalContentOffset()) / CharWidth);

        int line = pointerLine + _lineOffset;
        if (line >= _editor.LineCount)
        {
            line = _editor.LineCount - 1;
        }

        // this is a guard clause for when clicking within the designer
        // if we ever encounter a multiline string editor with no lines, we can
        // also evade that exception
        if (line < 0)
        {
            return;
        }

        int column = pointerColumn;
        int lineLength = _editor.LineLength(line);
        if (column > lineLength)
        {
            column = lineLength;
        }

        bool inRangeSelection = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        CaptureSelectionStart();
        CursorLineIndex = line;
        CursorCharacterIndex = column;
        HandleSelectionForCurrentNavigation(inRangeSelection);
        CapturePreferredCursorCharacter();
        e.Handled = true;
    }

    private void HandleSelectionForCurrentNavigation(bool inRangeSelection)
    {
        if (inRangeSelection)
        {
            _selectionSpan.SelectionEnd = _cursorLinePosition;
        }
        else
        {
            _selectionSpan.Clear();
        }
    }

    private void CaptureSelectionStart()
    {
        if (_selectionSpan.HasSelection)
            return;

        _selectionSpan.HasSelection = true;
        _selectionSpan.SelectionStart = _cursorLinePosition;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // adjust the min width here to ensure the line selection background fills out the displayed line
        codeEditorContent.MinWidth = availableSize.Width + extraDisplayWidth;
        UpdateEntireScroll();

        int previousCapacity = _lineBuffer.Capacity;
        int visibleLines = VisibleLines(availableSize);
        if (visibleLines > previousCapacity)
        {
            QueueUpdateVisibleText();
        }

        return base.MeasureOverride(availableSize);
    }

    private void GetCurrentTextPosition(out int line, out int column)
    {
        line = _cursorLinePosition.Line;
        column = _cursorLinePosition.Character;
    }

    private void BringCursorIntoView()
    {
        BringVerticalIntoView();
        BringHorizontalIntoView();
    }

    private void BringVerticalIntoView()
    {
        var lineIndex = CursorLineIndex;

        if (lineIndex < _lineOffset)
        {
            _lineOffset = lineIndex;
            UpdateLinesText();
        }
        else
        {
            int lastVisibleLine = _lineOffset + VisibleLines();
            int lastVisibleLineThreshold = lastVisibleLine - visibleLinesThreshold;
            if (lastVisibleLineThreshold < 0)
                return;

            int missingLines = lineIndex - lastVisibleLineThreshold;
            if (missingLines > 0)
            {
                _lineOffset += missingLines;
                UpdateLinesText();
            }
        }
    }

    private void BringHorizontalIntoView()
    {
        var selectedCursor = CurrentlyFocusedLine()!.cursor;
        var left = selectedCursor.LeftOffset;
        var contentBounds = codeCanvas.Bounds;
        var currentLeftOffset = GetHorizontalContentOffset();
        var relativeLeft = left - currentLeftOffset;
        double rightScrollThreshold = contentBounds.Right - scrollThresholdWidth;
        if (relativeLeft < scrollThresholdWidth)
        {
            var delta = relativeLeft - scrollThresholdWidth;
            var nextOffset = currentLeftOffset + delta;
            SetHorizontalContentOffset(nextOffset);
        }
        else if (relativeLeft > rightScrollThreshold)
        {
            var delta = relativeLeft - rightScrollThreshold;
            var nextOffset = currentLeftOffset + delta;
            SetHorizontalContentOffset(nextOffset);
        }
    }

    private CodeEditorLine? CurrentlyFocusedLine()
    {
        int lineIndex = _cursorLinePosition.Line;
        return _lineBuffer.GetLine(lineIndex);
    }

    private double GetHorizontalContentOffset()
    {
        return -Canvas.GetLeft(codeEditorContent);
    }

    private void SetHorizontalContentOffset(double value)
    {
        if (value < 0)
            value = 0;
        Canvas.SetLeft(codeEditorContent, -value);
        UpdateHorizontalScrollPosition();
    }

    private void UpdateLinesText()
    {
        ForceUpdateText();
        UpdateLinesDisplayPanel();
    }

    private void UpdateLinesQueueText()
    {
        QueueUpdateVisibleText();
        UpdateLinesDisplayPanel();
    }

    private void UpdateEntireScroll()
    {
        UpdateScrollBounds();
        UpdateHorizontalScrollPosition();
    }

    private void UpdateScrollBounds()
    {
        _isUpdatingScrollLimits = true;

        using (horizontalScrollBar.BeginUpdateBlock())
        {
            var maxWidth = codeEditorContent.Bounds.Width;
            horizontalScrollBar.MinValue = 0;
            horizontalScrollBar.MaxValue = maxWidth;
            horizontalScrollBar.SetAvailableScrollOnScrollableWindow();
        }

        using (verticalScrollBar.BeginUpdateBlock())
        {
            UpdateVerticalScroll();
        }

        _isUpdatingScrollLimits = false;
    }

    private void UpdateVerticalScroll()
    {
        int visibleLines = VisibleLines();
        verticalScrollBar.MaxValue = _editor.LineCount + visibleLines - 1;
        verticalScrollBar.StartPosition = _lineOffset;
        verticalScrollBar.EndPosition = _lineOffset + visibleLines;
        verticalScrollBar.SetAvailableScrollOnScrollableWindow();
    }

    private void UpdateHorizontalScrollPosition()
    {
        using (horizontalScrollBar.BeginUpdateBlock())
        {
            _isUpdatingScrollLimits = true;

            var start = GetHorizontalContentOffset();
            horizontalScrollBar.StartPosition = start;
            horizontalScrollBar.EndPosition = start + codeCanvas.Bounds.Width;

            _isUpdatingScrollLimits = false;
        }
    }

    private void RestartCursorAnimation()
    {
        var currentLine = _lineBuffer.GetLine(CursorLineIndex);
        if (currentLine is null)
            return;

        currentLine.RestartCursorAnimation();
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        const double scrollMultiplier = 50;
        const double verticalScrollMultiplier = scrollMultiplier / LineHeight;

        base.OnPointerWheelChanged(e);

        double steps = -e.Delta.Y * verticalScrollMultiplier;
        double verticalSteps = steps;
        double horizontalSteps = -e.Delta.X * scrollMultiplier;
        if (horizontalSteps is 0)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                horizontalSteps = verticalSteps;
                verticalSteps = 0;
            }
        }

        verticalScrollBar.Step(verticalSteps);
        horizontalScrollBar.Step(horizontalSteps);
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);
        InsertText(e.Text);
    }

    private void InsertText(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        GetCurrentTextPosition(out int line, out int column);
        _editor.InsertAt(line, column, text);
        CursorCharacterIndex += text.Length;
        UpdateVisibleTextTriggerCodeChanged();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        var modifiers = e.KeyModifiers;

        bool hasControl = modifiers.HasFlag(KeyModifiers.Control);
        bool hasShift = modifiers.HasFlag(KeyModifiers.Shift);

        switch (e.Key)
        {
            case Key.Back:
                if (hasControl)
                {
                    DeleteCommonCharacterGroupBackwards();
                    e.Handled = true;
                    break;
                }
                DeleteCurrentCharacterBackwards();
                e.Handled = true;
                break;

            case Key.Delete:
                if (hasControl)
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
                if (hasControl)
                {
                    if (hasShift)
                    {
                        _ = PasteDirectAsync();
                        e.Handled = true;
                        break;
                    }
                    _ = PasteClipboardTextAsync();
                    e.Handled = true;
                }
                break;

            case Key.C:
                if (hasControl)
                {
                    _ = CopySelectionToClipboardAsync();
                    e.Handled = true;
                }
                break;

            case Key.Left:
                if (hasControl)
                {
                    MoveCursorLeftWord();
                    e.Handled = true;
                    break;
                }
                MoveCursorLeft();
                e.Handled = true;
                break;

            case Key.Right:
                if (hasControl)
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
                if (hasControl)
                {
                    MoveCursorPageStart();
                    e.Handled = true;
                    break;
                }
                MoveCursorPageUp();
                e.Handled = true;
                break;

            case Key.PageDown:
                if (hasControl)
                {
                    MoveCursorPageEnd();
                    e.Handled = true;
                    break;
                }
                MoveCursorPageDown();
                e.Handled = true;
                break;

            case Key.Home:
                if (hasControl)
                {
                    MoveCursorDocumentStart();
                    e.Handled = true;
                    break;
                }
                MoveCursorLineStart();
                e.Handled = true;
                break;

            case Key.End:
                if (hasControl)
                {
                    MoveCursorDocumentEnd();
                    e.Handled = true;
                    break;
                }
                MoveCursorLineEnd();
                e.Handled = true;
                break;

            case Key.Tab:
                InsertTab();
                e.Handled = true;
                break;
        }

        base.OnKeyDown(e);
    }

    private readonly RateLimiter _pasteRateLimiter = new(TimeSpan.FromMilliseconds(400));

    private async Task PasteDirectAsync()
    {
        var canPaste = _pasteRateLimiter.Request();
        if (!canPaste)
            return;

        var text = await this.GetClipboardTextAsync();
        if (text is null)
            return;

        SetSource(text);
    }

    private void InsertTab()
    {
        const int tabSize = 4;
        var column = CursorCharacterIndex;
        int existingInTab = column % tabSize;
        int spacesToInsert = tabSize - existingInTab;
        InsertText(new string(' ', spacesToInsert));
    }

    private void MoveCursorPageStart()
    {
        MoveCursorToLine(_lineOffset);
    }

    private void MoveCursorPageEnd()
    {
        var offset = VisibleLines() - visibleLinesThreshold;
        var next = _lineOffset + offset;
        next = CapToLastLine(next);

        MoveCursorToLine(next);
    }

    private void MoveCursorPageUp()
    {
        var current = CursorLineIndex;
        var offset = VisibleLines();
        var next = current - offset;
        if (next < 0)
            next = 0;

        MoveCursorToLine(next);
    }

    private void MoveCursorPageDown()
    {
        var current = CursorLineIndex;
        var offset = VisibleLines();
        var next = current + offset;
        next = CapToLastLine(next);

        MoveCursorToLine(next);
    }

    private int CapToLastLine(int value)
    {
        if (value >= _editor.LineCount)
            return _editor.LineCount - 1;

        return value;
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
        MoveCursorToLine(CursorLineIndex + 1);
    }

    private void MoveCursorUp()
    {
        MoveCursorToLine(CursorLineIndex - 1);
    }

    private void MoveCursorToLine(int nextLine)
    {
        GetCurrentTextPosition(out var line, out var column);
        if (line == nextLine)
            return;

        if (nextLine < 0)
            return;

        if (nextLine >= _editor.LineCount)
            return;

        CursorLineIndex = nextLine;
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
        if (_cursorLinePosition.IsStart())
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

    private readonly AsyncLock _pasteLock = new();

    private async Task PasteClipboardTextAsync()
    {
        if (_pasteLock.IsLocked)
            return;

        using (_pasteLock.Lock())
        {
            // Get the positions before we paste the text
            // in which case the user might have buffered inputs that we
            // display before getting the content to paste,
            // therefore pasting the content on the position at which the
            // paste command was input
            // Will only break user input if the user inserts new input before
            // the paste location, which is rarer
            GetCurrentTextPosition(out int line, out int column);

            var pasteText = await this.GetClipboardTextAsync();
            if (pasteText is null)
                return;

            // TODO: Handle selection
            var nextCursorPosition = _editor.InsertAt(line, column, pasteText);
            CursorPosition = nextCursorPosition;
            UpdateVisibleTextTriggerCodeChanged();
        }
    }

    private void InsertLine()
    {
        GetCurrentTextPosition(out int line, out int column);
        _editor.InsertLineAtColumn(line, column);
        CursorLineIndex++;
        CursorCharacterIndex = 0;
        CapturePreferredCursorCharacter();
        UpdateVisibleTextTriggerCodeChanged();
    }

    private void DeleteCurrentCharacterBackwards()
    {
        if (_cursorLinePosition.IsStart())
            return;

        GetCurrentTextPosition(out int line, out int column);
        _editor.RemoveBackwardsAt(line, column, 1);
        MoveCursorLeft();
        CapturePreferredCursorCharacter();
        UpdateVisibleTextTriggerCodeChanged();
        RestartCursorAnimation();
    }

    private void DeleteCurrentCharacterForwards()
    {
        GetCurrentTextPosition(out int line, out int column);
        int lastLine = _editor.LineCount - 1;
        var lastLineLength = _editor.LineLength(lastLine);
        if (line == lastLine && column == lastLineLength)
            return;

        _editor.RemoveForwardsAt(line, column, 1);
        UpdateVisibleTextTriggerCodeChanged();
        RestartCursorAnimation();
    }

    private void DeleteCommonCharacterGroupBackwards()
    {
        if (_cursorLinePosition.IsStart())
            return;

        GetCurrentTextPosition(out int line, out int column);
        if ((line, column) is ( > 0, 0))
        {
            DeleteCurrentCharacterBackwards();
            return;
        }

        int previousColumn = column - 1;
        var start = LeftmostContiguousCommonCategory().Character;
        var lineContents = _editor.AtLine(line);
        var startChar = lineContents[start];
        var endChar = lineContents[previousColumn];
        bool coveringWhitespace =
            EditorCategory(startChar) is TextEditorCharacterCategory.Whitespace
            && EditorCategory(endChar) is TextEditorCharacterCategory.Whitespace;
        if (!coveringWhitespace)
        {
            // avoid removing the left whitespace
            var rightmostWhitespace = RightmostWhitespaceInCurrentLine(line, start);
            start = rightmostWhitespace.Character;
        }

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

        if (column >= currentLine.Length)
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
        if (end < currentLine.Length)
        {
            var leftmostWhitespace = LeftmostWhitespaceInCurrentLine(line, end);
            end = leftmostWhitespace.Character;
        }

        _editor.RemoveRangeInLine(line, column, end - 1);
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

            bool include = EvaluateContiguousCharacter(
                c,
                ref hasConsumedWhitespace,
                ref targetCategory,
                ref previousCategory);
            if (!include)
                break;

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

            bool include = EvaluateContiguousCharacter(
                c,
                ref hasConsumedWhitespace,
                ref targetCategory,
                ref previousCategory);
            if (!include)
                break;

            rightmost++;
        }

        return new(line, rightmost);
    }

    private static bool EvaluateContiguousCharacter(
        char c,
        ref bool hasConsumedWhitespace,
        ref TextEditorCharacterCategory targetCategory,
        ref TextEditorCharacterCategory previousCategory)
    {
        var category = EditorCategory(c);

        if (category is TextEditorCharacterCategory.Whitespace)
        {
            if (previousCategory is not TextEditorCharacterCategory.Whitespace)
            {
                if (hasConsumedWhitespace)
                    return false;
            }

            hasConsumedWhitespace = true;
        }
        else
        {
            if (category != previousCategory)
            {
                if (previousCategory is TextEditorCharacterCategory.Whitespace)
                {
                    if (hasConsumedWhitespace &&
                        targetCategory is not TextEditorCharacterCategory.Whitespace)
                    {
                        return false;
                    }
                }
            }

            // try to determine what char category we are seeking for
            if (targetCategory is TextEditorCharacterCategory.Whitespace)
            {
                targetCategory = category;
            }

            if (category != targetCategory)
            {
                return false;
            }
        }

        previousCategory = category;

        return true;
    }

    private LinePosition LeftmostWhitespaceInCurrentLine()
    {
        GetCurrentTextPosition(out int line, out int column);
        return RightmostWhitespaceInCurrentLine(line, column);
    }

    private LinePosition LeftmostWhitespaceInCurrentLine(int line, int column)
    {
        var currentLine = _editor.AtLine(line);
        int next = column;
        while (next > 0)
        {
            var c = currentLine[next];
            if (!char.IsWhiteSpace(c))
            {
                break;
            }

            column = next;
            next--;
        }

        return new(line, column);
    }

    private LinePosition RightmostWhitespaceInCurrentLine()
    {
        GetCurrentTextPosition(out int line, out int column);
        return RightmostWhitespaceInCurrentLine(line, column);
    }

    private LinePosition RightmostWhitespaceInCurrentLine(int line, int column)
    {
        var currentLine = _editor.AtLine(line);
        var currentLength = currentLine.Length;
        while (column < currentLength - 1)
        {
            var c = currentLine[column];
            if (!char.IsWhiteSpace(c))
            {
                break;
            }

            column++;
        }

        return new(line, column);
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

    public static double CharacterBeginPosition(int character)
    {
        return character * CharWidth + 1;
    }
}

public enum TextEditorCharacterCategory
{
    General,
    Identifier,
    Whitespace,
}
