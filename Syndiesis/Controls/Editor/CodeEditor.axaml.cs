using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.CodeAnalysis.Text;
using Syndiesis.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using static Syndiesis.Controls.CodeEditorLine;

namespace Syndiesis.Controls;

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
    public const double LineHeight = 20;
    public const double CharWidth = 8.6;

    private const double extraDisplayWidth = 200;
    private const double scrollThresholdWidth = extraDisplayWidth / 2;
    private const int visibleLinesThreshold = 2;

    private CursoredStringEditor _editor = new();
    private readonly CodeEditorLineBuffer _lineBuffer = new(20);

    private int _lineOffset;

    private SyntaxTreeListNode? _hoveredListNode;

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
        get => _editor.CursorLineIndex;
        set
        {
            _editor.CursorLineIndex = value;
        }
    }

    public int CursorCharacterIndex
    {
        get => _editor.CursorCharacterIndex;
        set
        {
            _editor.CursorCharacterIndex = value;
        }
    }

    public LinePosition CursorPosition
    {
        get => _editor.CursorPosition;
        set
        {
            _editor.CursorPosition = value;
        }
    }

    public CursoredStringEditor Editor
    {
        get => _editor;
        set
        {
            _editor = value;
            HandleCodeChanged();
            _editor.CodeChanged += HandleCodeChanged;
            _editor.CursorMoved += HandleCursorMoved;
        }
    }

    public SyntaxTreeListView? AssociatedTreeView { get; set; }

    private void HandleCursorMoved(LinePosition position)
    {
        UpdateCurrentContent();
    }

    private void HandleCodeChanged()
    {
        _hoveredListNode = null;
        UpdateCurrentContent();
    }

    private void UpdateCurrentContent()
    {
        UpdateVisibleCursor();
        UpdateVisibleText();
        BringCursorIntoView();
    }

    public event Action? CodeChanged
    {
        add => _editor.CodeChanged += value;
        remove => _editor.CodeChanged -= value;
    }
    public event Action<LinePosition>? CursorMoved
    {
        add => _editor.CursorMoved += value;
        remove => _editor.CursorMoved -= value;
    }

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
        _editor.SetSource(source);
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
        int bufferCapacity = totalVisible;
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

        _lineBuffer.LoadFrom(_lineOffset, _editor.MultilineEditor);
        int lineCount = _editor.LineCount;
        int visibleLines = VisibleLines();
        var lineRange = _lineBuffer.LineSpanForAbsoluteIndexRange(0, visibleLines);
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
        lineDisplayPanel.SelectedLineNumber = CursorLineIndex + 1;
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

        var span = _editor.SelectionLineSpan;
        SetHoverSpan(span, HighlightKind.Selection);
    }

    private IReadOnlyList<CodeEditorLine> CodeEditorLines()
    {
        return codeEditorContent.codeLinesPanel.Children
            .UpcastReadOnlyList<Control, CodeEditorLine>();
    }

    private void HideAllHoveredSyntaxNodes()
    {
        ClearAllHighlights(HighlightKind.SyntaxNodeHover);
    }

    private void HideAllTextSelection()
    {
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
        int lineLength = _editor.MultilineEditor.LineLength(line);
        if (column > lineLength)
        {
            column = lineLength;
        }

        bool inRangeSelection = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        _editor.SetSelectionMode(inRangeSelection);
        _editor.CursorPosition = new(line, column);
        _editor.CapturePreferredCursorCharacter();
        e.Handled = true;
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
        int lineIndex = CursorLineIndex;
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
        int lastLine = _editor.LineCount + visibleLines - 1;
        verticalScrollBar.MaxValue = Math.Max(0, lastLine);
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
        _editor.InsertText(e.Text);
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
                    _editor.DeleteCommonCharacterGroupBackwards();
                    e.Handled = true;
                    break;
                }
                _editor.DeleteCurrentCharacterBackwards();
                e.Handled = true;
                break;

            case Key.Delete:
                if (hasControl)
                {
                    _editor.DeleteCommonCharacterGroupForwards();
                    e.Handled = true;
                    break;
                }
                _editor.DeleteCurrentCharacterForwards();
                e.Handled = true;
                break;

            case Key.Enter:
                _editor.InsertLine();
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
                    _ = CopySelectionToClipboardAsync()
                        .ConfigureAwait(false);
                    e.Handled = true;
                }
                break;

            case Key.X:
                if (hasControl)
                {
                    _ = CutSelectionToClipboardAsync()
                        .ConfigureAwait(false);
                    e.Handled = true;
                }
                break;

            case Key.W:
                if (hasControl)
                {
                    SelectCurrentWord();
                    e.Handled = true;
                }
                break;

            case Key.U:
                if (hasControl)
                {
                    ExpandSelectNextNode();
                    e.Handled = true;
                }
                break;

            case Key.A:
                if (hasControl)
                {
                    SelectAll();
                    e.Handled = true;
                }
                break;

            case Key.Left:
                _editor.SetSelectionMode(hasShift);
                if (hasControl)
                {
                    _editor.MoveCursorLeftWord();
                    e.Handled = true;
                    break;
                }
                _editor.MoveCursorLeft();
                e.Handled = true;
                break;

            case Key.Right:
                _editor.SetSelectionMode(hasShift);
                if (hasControl)
                {
                    _editor.MoveCursorNextWord();
                    e.Handled = true;
                    break;
                }
                _editor.MoveCursorRight();
                e.Handled = true;
                break;

            case Key.Up:
                _editor.SetSelectionMode(hasShift);
                _editor.MoveCursorUp();
                e.Handled = true;
                break;

            case Key.Down:
                _editor.SetSelectionMode(hasShift);
                _editor.MoveCursorDown();
                e.Handled = true;
                break;

            case Key.PageUp:
                _editor.SetSelectionMode(hasShift);
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
                _editor.SetSelectionMode(hasShift);
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
                _editor.SetSelectionMode(hasShift);
                if (hasControl)
                {
                    _editor.MoveCursorDocumentStart();
                    e.Handled = true;
                    break;
                }
                _editor.MoveCursorLineStart();
                e.Handled = true;
                break;

            case Key.End:
                _editor.SetSelectionMode(hasShift);
                if (hasControl)
                {
                    _editor.MoveCursorDocumentEnd();
                    e.Handled = true;
                    break;
                }
                _editor.MoveCursorLineEnd();
                e.Handled = true;
                break;

            case Key.Tab:
                if (hasShift)
                {
                    _editor.ReduceIndentation();
                    e.Handled = true;
                    break;
                }
                HandleTabInsertion();
                e.Handled = true;
                break;
        }

        base.OnKeyDown(e);
    }

    private void HandleTabInsertion()
    {
        if (!_editor.HasSelection)
        {
            _editor.InsertTab();
        }
        else
        {
            _editor.IncreaseIndentation();
        }
    }

    private void ExpandSelectNextNode()
    {
        var discovered = DiscoverParentNodeCoveringSelection();
        if (discovered is null)
            return;

        var span = discovered.NodeLine.DisplayLineSpan;
        _editor.SelectionLineSpan = span;
        _editor.InvertSelectionCursorPosition();
        AssociatedTreeView?.OverrideHover(discovered);
    }

    private SyntaxTreeListNode? DiscoverParentNodeCoveringSelection()
    {
        var span = _editor.SelectionLineSpan;
        var start = span.Start;
        var end = span.End;

        var startIndex = _editor.MultilineEditor.GetIndex(start);
        var endIndex = _editor.MultilineEditor.GetIndex(end);

        return AssociatedTreeView?.DiscoverParentNodeCoveringSelection(startIndex, endIndex);
    }

    private void SelectCurrentWord()
    {
        _editor.SetSelectionMode(false);
        _editor.MoveCursorNextWord();
        _editor.SetSelectionMode(true);
        _editor.MoveCursorLeftWord();
        _editor.InvertSelectionCursorPosition();
    }

    private void MoveCursorPageStart()
    {
        _editor.MoveCursorToLine(_lineOffset);
    }

    private void MoveCursorPageEnd()
    {
        var offset = VisibleLines() - visibleLinesThreshold;
        var next = _lineOffset + offset;
        next = CapToLastLine(next);

        _editor.MoveCursorToLine(next);
    }

    private void MoveCursorPageUp()
    {
        var current = CursorLineIndex;
        var offset = VisibleLines();
        var next = current - offset;
        if (next < 0)
            next = 0;

        _editor.MoveCursorToLine(next);
    }

    private void MoveCursorPageDown()
    {
        var current = CursorLineIndex;
        var offset = VisibleLines();
        var next = current + offset;
        next = CapToLastLine(next);

        _editor.MoveCursorToLine(next);
    }

    private int CapToLastLine(int value)
    {
        if (value >= _editor.LineCount)
            return _editor.LineCount - 1;

        return value;
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

    private void SelectAll()
    {
        _editor.SelectAll();
    }

    private async Task CutSelectionToClipboardAsync()
    {
        var selection = _editor.GetCurrentSelectionString();
        if (string.IsNullOrEmpty(selection))
            return;

        _editor.DeleteCurrentSelection();
        await this.SetClipboardTextAsync(selection)
            .ConfigureAwait(false);
    }

    private async Task CopySelectionToClipboardAsync()
    {
        var selection = _editor.GetCurrentSelectionString();
        if (string.IsNullOrEmpty(selection))
            return;

        await this.SetClipboardTextAsync(selection)
            .ConfigureAwait(false);
    }

    private readonly AsyncUsableLock _pasteLock = new();

    private async Task PasteClipboardTextAsync()
    {
        if (_pasteLock.IsLocked)
            return;

        using (_pasteLock.Lock())
        {
            var pasteText = await this.GetClipboardTextAsync();
            if (pasteText is null)
                return;

            _editor.InsertText(pasteText);
            QueueUpdateVisibleText();
        }
    }

    public static double CharacterBeginPosition(int character)
    {
        return character * CharWidth + 1;
    }
}
