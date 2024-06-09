using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Editor;
using Syndiesis.Core;
using Syndiesis.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Syndiesis.Controls;

/// <summary>
/// A code editor using <see cref="TextEditor"/>.
/// </summary>
/// <remarks>
/// It does not yet provide support for IDE features like autocompletion, code fixes,
/// or others. They may be added in the future.
/// </remarks>
public partial class CodeEditor : UserControl
{
    private const double extraDisplayWidth = 200;

    private AnalysisTreeListNode? _hoveredListNode;

    private bool _isUpdatingScrollLimits = false;
    private int _disabledNodeHoverTimes;

    private NodeSpanHoverLayer _nodeSpanHoverLayer;
    private RoslynColorizerContainer? _roslynColorizer;

    private HybridSingleTreeCompilationSource? _compilationSource;
    private RoslynColorizer? _effectiveColorizer;

    public AnalysisTreeListView? AssociatedTreeView { get; set; }

    public HybridSingleTreeCompilationSource? CompilationSource
    {
        get => _compilationSource;
        set
        {
            _compilationSource = value;
            if (value is not null)
            {
                _roslynColorizer = new RoslynColorizerContainer(value);
                _effectiveColorizer = _roslynColorizer.EffectiveColorizer;
                textEditor.TextArea.TextView.LineTransformers.Add(_effectiveColorizer);
            }
            else
            {
                _roslynColorizer = null;
                _effectiveColorizer = null;
                textEditor.TextArea.TextView.LineTransformers.Remove(_effectiveColorizer);
            }
        }
    }

    public TextViewPosition CaretPosition
    {
        get => textEditor.TextArea.Caret.Position;
        set => textEditor.TextArea.Caret.Position = value;
    }

    public SimpleSegment HoveredListNodeSegment { get; private set; }

    public TextDocument Document
    {
        get => textEditor.Document;
        set
        {
            textEditor.Document = value;
            HandleNewDocument();
        }
    }

    public bool ColorizerEnabled
    {
        get => _roslynColorizer?.Enabled ?? false;
        set
        {
            if (_roslynColorizer is null)
                return;

            _roslynColorizer.Enabled = value;
        }
    }

    public event EventHandler? TextChanged
    {
        add => textEditor.Document.TextChanged += value;
        remove => textEditor.Document.TextChanged -= value;
    }
    public event EventHandler? CaretMoved
    {
        add => textEditor.TextArea.Caret.PositionChanged += value;
        remove => textEditor.TextArea.Caret.PositionChanged -= value;
    }
    public event EventHandler? SelectionChanged
    {
        add => textEditor.TextArea.SelectionChanged += value;
        remove => textEditor.TextArea.SelectionChanged -= value;
    }

    public event Action? AnalysisCompleted;

    public CodeEditor()
    {
        InitializeComponent();
        InitializeEvents();
        InitializeTextEditor();
    }

    [MemberNotNull(nameof(_nodeSpanHoverLayer))]
    private void InitializeTextEditor()
    {
        var textArea = textEditor.TextArea;
        textArea.SelectionBrush =
            new LinearGradientBrush()
            {
                StartPoint = new(0, 0, RelativeUnit.Relative),
                EndPoint = new(0, 1, RelativeUnit.Relative),
                GradientStops =
                {
                    new(Color.FromUInt32(0x60165A99), 0),
                    new(Color.FromUInt32(0x600090FF), 1),
                }
            };

        _nodeSpanHoverLayer = new NodeSpanHoverLayer(this)
        {
            HoverForeground = new SolidColorBrush(0x60A0A0A0)
        };

        textArea.TextView.InsertLayer(
            _nodeSpanHoverLayer,
            KnownLayer.Selection,
            LayerInsertionPosition.Above);

        textArea.LeftMargins[0] = new BackgroundLineNumberPanel(textArea.TextView);
    }

    private void InitializeEvents()
    {
        verticalScrollBar.ScrollChanged += OnVerticalScroll;
        horizontalScrollBar.ScrollChanged += OnHorizontalScroll;

        Document.TextChanged += HandleTextChanged;
        textEditor.TextArea.Caret.PositionChanged += HandleCaretPositionChanged;
        textEditor.Loaded += HandleTextEditorLoaded;
    }

    private void HandleTextEditorLoaded(object? sender, RoutedEventArgs e)
    {
        var scrollViewer = GetTextEditorScrollViewer();
        scrollViewer!.ScrollChanged += HandleScrollOffsetChanged;
    }

    private void HandleNewDocument()
    {
        Document.TextChanged += HandleTextChanged;
    }

    private void HandleTextChanged(object? sender, EventArgs e)
    {
        HandleCodeChanged();
    }

    private void HandleCaretPositionChanged(object? sender, EventArgs e)
    {
        //EnsureHorizontalScrollFitsCaret();
    }

    // This is actually hopeless
    private void EnsureHorizontalScrollFitsCaret()
    {
        var rectangle = textEditor.TextArea.Caret.CalculateCaretRectangle();
        var right = rectangle.Right;
        var scrollViewer = GetTextEditorScrollViewer();

        if (scrollViewer is null)
            return;

        double previousOffset = scrollViewer.Offset.X;
        double maxRight = previousOffset + scrollViewer.Viewport.Width;
        const double rightOffset = 70;
        var offset = maxRight - right;
        if (offset < rightOffset)
        {
            var missing = rightOffset - offset;
            scrollViewer.SetHorizontalOffset(previousOffset + missing);
        }
    }

    private void HandleScrollOffsetChanged(object? sender, EventArgs e)
    {
        UpdateScrolls();
    }

    private void HandleCodeChanged()
    {
        _hoveredListNode = null;
        if (AssociatedTreeView is not null)
        {
            AssociatedTreeView.AnalyzedTree = null;
        }
        ColorizerEnabled = false;

        ClearHoverSpan();
        _nodeSpanHoverLayer.InvalidateVisual();
    }

    private void UpdateCurrentColorizer()
    {
        var nextEffective = _roslynColorizer?.EffectiveColorizer;
        if (_effectiveColorizer == nextEffective)
            return;

        textEditor.TextArea.TextView.LineTransformers.Remove(_effectiveColorizer);
        textEditor.TextArea.TextView.LineTransformers.Add(nextEffective);

        _effectiveColorizer = nextEffective;
    }

    public void RegisterAnalysisPipelineHandler(
        AnalysisPipelineHandler analysisPipelineHandler)
    {
        analysisPipelineHandler.AnalysisCompleted += HandleAnalysisCompleted;
    }

    private void HandleAnalysisCompleted(AnalysisResult result)
    {
        void UIUpdate()
        {
            UpdateCurrentColorizer();
            ColorizerEnabled = true;
            textEditor.TextArea.TextView.Redraw();
        }

        AnalysisCompleted?.Invoke();
        Dispatcher.UIThread.InvokeAsync(UIUpdate);
    }

    private void OnVerticalScroll()
    {
        if (_isUpdatingScrollLimits)
            return;

        var scrollViewer = GetTextEditorScrollViewer();
        scrollViewer?.SetVerticalOffset(verticalScrollBar.StartPosition);
    }

    private void OnHorizontalScroll()
    {
        if (_isUpdatingScrollLimits)
            return;

        var scrollViewer = GetTextEditorScrollViewer();
        scrollViewer?.SetHorizontalOffset(horizontalScrollBar.StartPosition);
    }

    private ScrollViewer? GetTextEditorScrollViewer()
    {
        return _scrollViewerProperty.GetValue(textEditor) as ScrollViewer;
    }

    private static readonly PropertyInfo _scrollViewerProperty
        = typeof(TextEditor)
            .GetProperty(
                "ScrollViewer",
                BindingFlags.NonPublic | BindingFlags.Instance)!
            ;

    public void SetSource(string source)
    {
        ColorizerEnabled = false;
        textEditor.Document.Text = source;
    }

    public void ApplySettings(AppSettings settings)
    {
        ApplyIndentationOptions(settings.IndentationOptions);

        var editorOptions = textEditor.Options;
        editorOptions.ShowSpaces = settings.ShowWhitespaceGlyphs;
        editorOptions.ShowTabs = settings.ShowWhitespaceGlyphs;
        editorOptions.ShowEndOfLine = settings.ShowWhitespaceGlyphs;
        editorOptions.ShowBoxForControlCharacters = settings.ShowWhitespaceGlyphs;
        editorOptions.WordWrapIndentation = 120;
        textEditor.WordWrap = settings.WordWrap;
    }

    public void ApplyIndentationOptions(IndentationOptions options)
    {
        var areaOptions = textEditor.TextArea.Options;
        areaOptions.ConvertTabsToSpaces = options.WhitespaceKind is WhitespaceKind.Space;
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

        ClearHoverSpan();

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

        _nodeSpanHoverLayer.InvalidateVisual();
    }

    public void PlaceCursorAtNodeStart(AnalysisTreeListNode node)
    {
        if (node is not { AssociatedSyntaxObject: not null and var syntaxObject })
            return;

        var tree = AssociatedTreeView!.AnalyzedTree;
        var start = syntaxObject.GetLineSpan(tree).Start;
        _disabledNodeHoverTimes++;
        SetCaretPositionBringToView(start.TextViewPosition());
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
        SetCaretPositionBringToView(lineSpan.Start.TextViewPosition());
    }

    private void SetCaretPositionBringToView(LinePosition position)
    {
        SetCaretPositionBringToView(position.TextViewPosition());
    }

    private void SetCaretPositionBringToView(TextViewPosition position)
    {
        CaretPosition = position;
        textEditor.TextArea.Caret.BringCaretToView();
    }

    private void ClearHoverSpan()
    {
        HoveredListNodeSegment = default;
    }

    private void SetHoverSpan(SimpleSegment segment)
    {
        var area = textEditor.TextArea;
        int length = area.TextView.Document.TextLength;
        segment = segment.ConfineToBounds(length);
        HoveredListNodeSegment = segment;
    }

    private void UpdateScrolls()
    {
        _isUpdatingScrollLimits = true;

        using (horizontalScrollBar.BeginUpdateBlock())
        {
            var maxWidth = textEditor.ExtentWidth;
            var start = textEditor.HorizontalOffset;
            horizontalScrollBar.MinValue = 0;
            horizontalScrollBar.MaxValue = maxWidth;
            horizontalScrollBar.StartPosition = start;
            horizontalScrollBar.EndPosition = start + textEditor.ViewportWidth;
            horizontalScrollBar.SetAvailableScrollOnScrollableWindow();
        }

        using (verticalScrollBar.BeginUpdateBlock())
        {
            verticalScrollBar.MinValue = 0;
            verticalScrollBar.MaxValue = textEditor.ExtentHeight;
            verticalScrollBar.StartPosition = textEditor.VerticalOffset;
            verticalScrollBar.EndPosition = textEditor.VerticalOffset + textEditor.ViewportHeight;
            verticalScrollBar.SetAvailableScrollOnScrollableWindow();
        }

        _isUpdatingScrollLimits = false;
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        const double scrollAmplificationMultiplier = 60;

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
            case Key.U:
                if (modifiers is KeyModifiers.Control)
                {
                    ExpandSelectNextParentNode();
                    e.Handled = true;
                }
                break;

            case Key.V:
                if (modifiers is (KeyModifiers.Shift | KeyModifiers.Control))
                {
                    Dispatcher.UIThread.InvokeAsync(PasteDirectAsync);
                    e.Handled = true;
                }
                break;

            case Key.W:
                if (modifiers is KeyModifiers.Control)
                {
                    SelectCurrentWord();
                    e.Handled = true;
                }
                break;

            case Key.F12:
                if (modifiers is KeyModifiers.None)
                {
                    GoToDefinition();
                    e.Handled = true;
                }
                break;
        }

        if (e.Handled)
            return;

        base.OnKeyDown(e);
    }

    private readonly CancellationTokenFactory _pulseFactoryCancellationToken = new();

    private void GoToDefinition()
    {
        var went = TryGoToDefinition();
        if (!went)
        {
            _pulseFactoryCancellationToken.Cancel();
            var pulseAnimation = Animations.CreateColorPulseAnimation(
                backgroundPanel,
                Color.FromUInt32(0xFF440011),
                Panel.BackgroundProperty);
            pulseAnimation.Duration = TimeSpan.FromMilliseconds(250);

            _ = pulseAnimation.RunAsync(backgroundPanel, _pulseFactoryCancellationToken.CurrentToken);
        }
    }

    private bool TryGoToDefinition()
    {
        var symbol = DiscoverSymbolAtCaret();
        if (symbol is null)
            return false;

        if (symbol is IMethodSymbol
            { 
                MethodKind: MethodKind.Constructor,
                IsImplicitlyDeclared: true
            } constructor)
        {
            symbol = constructor.ContainingType;
        }

        var syntax = symbol.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntax is null)
            return false;

        var targetSpeakableName = symbol.Name;
        if (symbol is IMethodSymbol { MethodKind: MethodKind.Constructor })
        {
            targetSpeakableName = symbol.ContainingType.Name;
        }

        var syntaxNode = syntax.GetSyntax();
        var tokens = syntaxNode.DescendantTokens();
        var token = tokens.FirstOrDefault(s => s.Text == targetSpeakableName);
        if (token == default)
            return false;

        var span = token.Span;
        textEditor.Select(span.Start, span.Length);
        textEditor.TextArea.Caret.BringCaretToView();
        return true;
    }

    private ISymbol? DiscoverSymbolAtCaret()
    {
        var source = CompilationSource?.CurrentSource;
        if (source is null)
            return null;

        var tree = source.Tree;
        if (tree is null)
            return null;

        var model = source.SemanticModel!;
        int position = textEditor.TextArea.Caret.Offset;
        var node = SyntaxNodeAtPosition(tree, position);
        var symbol = GetSymbol(node);
        if (symbol is null)
        {
            // Attempt at the left side of the caret
            var otherNode = SyntaxNodeAtPosition(tree, position - 1);
            var otherSymbol = GetSymbol(otherNode);
            return otherSymbol;
        }

        return symbol;

        ISymbol? GetSymbol(SyntaxNode? node)
        {
            if (node is null)
                return default;

            var symbolInfo = model.GetSymbolInfo(node);
            return symbolInfo.Symbol
                ?? symbolInfo.CandidateSymbols.FirstOrDefault()
                ?? model.GetDeclaredSymbol(node);
        }
    }

    private SyntaxNode? SyntaxNodeAtPosition(SyntaxTree tree, int position)
    {
        var root = tree.GetRoot();
        return root.DeepestNodeContainingPosition(position);
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

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        UpdateScrolls();
        base.OnSizeChanged(e);
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
