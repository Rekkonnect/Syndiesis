using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Core;
using Syndiesis.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Syndiesis.Controls;

/// <summary>
/// A code editor using <see cref="TextEditor"/>.
/// </summary>
/// <remarks>
/// It does not yet provide support for IDe features like autocompletion, code fixes,
/// or others. They may be added in the future
/// </remarks>
public partial class CodeEditor : UserControl
{
    private const double extraDisplayWidth = 200;

    private AnalysisTreeListNode? _hoveredListNode;

    private bool _isUpdatingScrollLimits = false;
    private int _disabledNodeHoverTimes;

    public AnalysisTreeListView? AssociatedTreeView { get; set; }

    public TextViewPosition CaretPosition
    {
        get => textEditor.TextArea.Caret.Position;
        set => textEditor.TextArea.Caret.Position = value;
    }

    public event EventHandler? CodeChanged
    {
        add => textEditor.Document.TextChanged += value;
        remove => textEditor.Document.TextChanged -= value;
    }
    public event EventHandler? CaretMoved
    {
        add => textEditor.TextArea.Caret.PositionChanged += value;
        remove => textEditor.TextArea.Caret.PositionChanged -= value;
    }

    public CodeEditor()
    {
        InitializeComponent();
        InitializeEvents();
        InitializeTextEditor();
    }

    private void InitializeTextEditor()
    {
        textEditor.TextArea.SelectionBrush =
            new LinearGradientBrush()
            {
                StartPoint = new(0, 0, RelativeUnit.Relative),
                EndPoint = new(0, 1, RelativeUnit.Relative),
                GradientStops =
                {
                    new(Color.FromUInt32(0x60164099), 0),
                    new(Color.FromUInt32(0x600090FF), 1),
                }
            };

        var lineNumberMargin = textEditor.TextArea.LeftMargins.OfType<LineNumberMargin>()
            .FirstOrDefault();
        if (lineNumberMargin is not null)
        {
            lineNumberMargin.Margin = new(20, 0, 0, 0);
        }
    }

    private void InitializeEvents()
    {
        verticalScrollBar.ScrollChanged += OnVerticalScroll;
        horizontalScrollBar.ScrollChanged += OnHorizontalScroll;

        textEditor.TextArea.Caret.PositionChanged += HandleCaretPositionChanged;
    }

    private void HandleCaretPositionChanged(object? sender, EventArgs e)
    {
        UpdateEntireScroll();
    }

    private void HandleCodeChanged()
    {
        _hoveredListNode = null;
        if (AssociatedTreeView is not null)
        {
            AssociatedTreeView.AnalyzedTree = null;
        }

    }

    private void OnVerticalScroll()
    {
        if (_isUpdatingScrollLimits)
            return;

        textEditor.ScrollToVerticalOffset(verticalScrollBar.StartPosition);
    }

    private void OnHorizontalScroll()
    {
        if (_isUpdatingScrollLimits)
            return;

        var offset = horizontalScrollBar.StartPosition;
    }

    public void SetSource(string source)
    {
        textEditor.Document.Text = source;
    }

    private bool _hasRequestedTextUpdate = false;

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        _hasRequestedTextUpdate = true;
        base.OnSizeChanged(e);
    }

    private void ShowSelectedLine()
    {

    }

    public void ApplyIndentationOptions(IndentationOptions options)
    {
        var areaOptions = textEditor.TextArea.Options;
        areaOptions.ConvertTabsToSpaces = options.WhitespaceKind is Core.WhitespaceKind.Space;
        areaOptions.IndentationSize = options.IndentationWidth;
    }

    public void ShowHoveredSyntaxNode(AnalysisTreeListNode? listNode)
    {
        _hoveredListNode = listNode;
        ShowCurrentHoveredSyntaxNode();
    }

    private void ShowCurrentHoveredSyntaxNode()
    {
        if (_disabledNodeHoverTimes > 0)
        {
            _disabledNodeHoverTimes--;
            return;
        }

        HideAllHoveredSyntaxNodes();

        if (_hoveredListNode is not null)
        {
            var current = _hoveredListNode;
            if (current is not null)
            {
                var currentLine = current.NodeLine;
                var tree = AssociatedTreeView!.AnalyzedTree;
                var span = currentLine.DisplaySpan;
                var segment = new SimpleSegment(span.Start, span.Length);
                SetHoverSpan(segment);
            }
        }
    }

    public void PlaceCursorAtNodeStart(AnalysisTreeListNode node)
    {
        if (node is not { AssociatedSyntaxObject: not null and var syntaxObject })
            return;

        var tree = AssociatedTreeView!.AnalyzedTree;
        var start = syntaxObject.GetLineSpan(tree).Start;
        _disabledNodeHoverTimes++;
        CaretPosition = start.TextViewPosition();
    }

    public void SelectTextOfNode(AnalysisTreeListNode node)
    {
        if (node is not { AssociatedSyntaxObject: not null and var syntaxObject })
            return;

        var tree = AssociatedTreeView!.AnalyzedTree;
        var lineSpan = syntaxObject.GetLineSpan(tree);
        _disabledNodeHoverTimes++;

        var document = textEditor.Document;
        var segment = document.GetSegment(lineSpan);
        var area = textEditor.TextArea;
        area.Selection = Selection.Create(area, segment);
    }

    private AnalysisTreeListNode? DeepestWithSyntaxObject(AnalysisTreeListNode? node)
    {
        if (node is null)
            return null;

        var sourceKind = node.NodeLine.AnalysisNodeKind;
        var current = node;
        var previous = node;
        while (current is not null)
        {
            if (current.NodeLine.AnalysisNodeKind != sourceKind)
            {
                return previous;
            }

            var currentLine = current.NodeLine;
            var syntaxObject = currentLine.AssociatedSyntaxObject;
            if (syntaxObject is not null)
            {
                return current;
            }

            previous = current;
            current = current.ParentNode;
        }

        return null;
    }

    private void HideAllHoveredSyntaxNodes()
    {
        ClearAllHighlights();
    }

    private void ClearAllHighlights()
    {
        // TODO
    }

    private void SetHoverSpan(SimpleSegment segment)
    {
        var area = textEditor.TextArea;
        int length = area.TextView.Document.TextLength;
        segment = segment.ConfineToBounds(length);
        var selection = Selection.Create(area, segment);
        // TODO: Show syntax highlighting
        //textEditor.SyntaxHighlighting
    }

    private void UpdateEntireScroll()
    {
        // TODO: Unify the updating methods below
        // + ensure the scroll info is properly retrieved
        // right now it's always late by one trigger
        UpdateScrollBounds();
        UpdateHorizontalScrollPosition();
    }

    private void UpdateScrollBounds()
    {
        _isUpdatingScrollLimits = true;

        using (horizontalScrollBar.BeginUpdateBlock())
        {
            var maxWidth = textEditor.ExtentWidth;
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
        verticalScrollBar.MinValue = 0;
        verticalScrollBar.MaxValue = textEditor.ExtentHeight;
        verticalScrollBar.StartPosition = textEditor.VerticalOffset;
        verticalScrollBar.EndPosition = textEditor.VerticalOffset + textEditor.ViewportHeight;
        verticalScrollBar.SetAvailableScrollOnScrollableWindow();
    }

    private void UpdateHorizontalScrollPosition()
    {
        using (horizontalScrollBar.BeginUpdateBlock())
        {
            _isUpdatingScrollLimits = true;

            var start = textEditor.HorizontalOffset;
            horizontalScrollBar.StartPosition = start;
            horizontalScrollBar.EndPosition = start + textEditor.ViewportWidth;

            _isUpdatingScrollLimits = false;
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        const double scrollAmplificationMultiplier = 30;

        base.OnPointerWheelChanged(e);

        double verticalSteps = -e.Delta.Y * scrollAmplificationMultiplier;
        double horizontalSteps = -e.Delta.X * scrollAmplificationMultiplier;
        if (horizontalSteps is 0)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                horizontalSteps = verticalSteps;
            }
        }

        horizontalScrollBar.Step(horizontalSteps);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        var modifiers = e.KeyModifiers.NormalizeByPlatform();

        switch (e.Key)
        {
            case Key.W:
                if (modifiers is KeyModifiers.Control)
                {
                    SelectCurrentWord();
                    e.Handled = true;
                }
                break;

            case Key.U:
                if (modifiers is KeyModifiers.Control)
                {
                    ExpandSelectNextParentNode();
                    e.Handled = true;
                }
                break;
        }

        base.OnKeyDown(e);
    }

    private void SelectCurrentWord()
    {
        var segment = GetCurrentWordSegment();
        textEditor.Select(segment.Offset, segment.Length);
    }

    private SimpleSegment GetCurrentWordSegment()
    {
        var area = textEditor.TextArea;
        bool enableVirtualSpace = area.Selection.EnableVirtualSpace;
        var visualColumn = area.Caret.VisualColumn;
        var line = area.TextView.GetVisualLine(area.Caret.Line);

        // Copied from SelectionMouseHandler from AvaloniaEdit -- there is no
        // exposed command for selecting the word of the current caret position
        var wordStartVisualColumn = line.GetNextCaretPosition(
            visualColumn + 1,
            LogicalDirection.Backward,
            CaretPositioningMode.WordStartOrSymbol,
            enableVirtualSpace);
        if (wordStartVisualColumn == -1)
            wordStartVisualColumn = 0;

        var wordEndVisualColumn = line.GetNextCaretPosition(
            wordStartVisualColumn,
            LogicalDirection.Forward,
            CaretPositioningMode.WordBorderOrSymbol,
            enableVirtualSpace);
        if (wordEndVisualColumn == -1)
            wordEndVisualColumn = line.VisualLength;

        var relativeOffset = line.FirstDocumentLine.Offset;
        var wordStartOffset = line.GetRelativeOffset(wordStartVisualColumn) + relativeOffset;
        var wordEndOffset = line.GetRelativeOffset(wordEndVisualColumn) + relativeOffset;
        return new SimpleSegment(wordStartOffset, wordEndOffset - wordStartOffset);
    }

    private void ExpandSelectNextParentNode()
    {
        var discovered = DiscoverParentNodeCoveringSelection();
        if (discovered is null)
            return;

        var tree = AssociatedTreeView!.AnalyzedTree;
        var span = discovered.NodeLine.DisplaySpan;
        textEditor.Select(span.Start, span.Length);
        AssociatedTreeView?.OverrideHover(discovered);
    }

    private AnalysisTreeListNode? DiscoverParentNodeCoveringSelection()
    {
        var surrounding = CurrentSelectionOrCaretSegment();
        int start = surrounding.Offset;
        int end = surrounding.EndOffset;

        return AssociatedTreeView?.DiscoverParentNodeCoveringSelection(start, end);
    }

    private ISegment CurrentSelectionOrCaretSegment()
    {
        var selection = textEditor.TextArea.Selection;
        if (selection.IsEmpty)
        {
            var caret = textEditor.CaretOffset;
            return new SimpleSegment(caret, 0);
        }

        return selection.SurroundingSegment;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = base.ArrangeOverride(finalSize);
        UpdateEntireScroll();
        return result;
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
}
