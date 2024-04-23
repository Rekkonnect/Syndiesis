using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;

namespace CSharpSyntaxEditor.Controls;

public partial class SyntaxTreeListView : UserControl
{
    private bool _allowedHover;
    private SyntaxTreeListNode? _hoveredNode;

    public static readonly StyledProperty<SyntaxTreeListNode> RootNodeProperty =
        AvaloniaProperty.Register<CodeEditorLine, SyntaxTreeListNode>(
            nameof(RootNode),
            defaultValue: new());

    public SyntaxTreeListNode RootNode
    {
        get => GetValue(RootNodeProperty);
        set
        {
            SetValue(RootNodeProperty, value);
            topLevelNodeContent.Content = value;

            value.SetListViewRecursively(this);
            value.SizeChanged += HandleRootNodeSizeAdjusted;
            UpdateRootChanged();
        }
    }

    private void HandleRootNodeSizeAdjusted(object? sender, SizeChangedEventArgs e)
    {
        UpdateScrollLimits();
    }

    private void UpdateRootChanged()
    {
        RootNode.Loaded += NewRootNodeLoaded;
    }

    private void NewRootNodeLoaded(object? sender, RoutedEventArgs e)
    {
        RootNode.Loaded -= NewRootNodeLoaded;
        UpdateScrollLimits();
        CorrectContainedNodeWidths(Bounds.Size);
    }

    public SyntaxTreeListView()
    {
        InitializeComponent();
        InitializeEvents();
    }

    protected override void ArrangeCore(Rect finalRect)
    {
        base.ArrangeCore(finalRect);
        UpdateScrollLimits();
    }

    private void UpdateScrollLimits()
    {
        var node = RootNode;

        verticalScrollBar.BeginUpdate();
        verticalScrollBar.MaxValue = node.Bounds.Height + 50;
        verticalScrollBar.EndPosition = verticalScrollBar.StartPosition + codeCanvas.Bounds.Height;
        verticalScrollBar.HasAvailableScroll = !verticalScrollBar.HasFullRangeWindow;
        verticalScrollBar.EndUpdate();

        horizontalScrollBar.BeginUpdate();
        horizontalScrollBar.MaxValue = node.Bounds.Width;
        horizontalScrollBar.EndPosition = horizontalScrollBar.StartPosition + codeCanvas.Bounds.Width;
        horizontalScrollBar.HasAvailableScroll = !horizontalScrollBar.HasFullRangeWindow;
        horizontalScrollBar.EndUpdate();
    }

    private void InitializeEvents()
    {
        verticalScrollBar.ScrollChanged += OnVerticalScroll;
        horizontalScrollBar.ScrollChanged += OnHorizontalScroll;
    }

    private void OnVerticalScroll()
    {
        var top = verticalScrollBar.StartPosition;
        Canvas.SetTop(topLevelNodeContent, -top);
        InvalidateArrange();
    }

    private void OnHorizontalScroll()
    {
        var left = horizontalScrollBar.StartPosition;
        Canvas.SetLeft(topLevelNodeContent, -left);
        InvalidateArrange();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        EvaluateHovering(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        EvaluateHovering(e);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        const double scrollMultiplier = 50;

        base.OnPointerWheelChanged(e);

        var pointerPosition = e.GetCurrentPoint(this).Position;
        if (!codeCanvasContainer.Bounds.Contains(pointerPosition))
        {
            return;
        }

        double steps = -e.Delta.Y * scrollMultiplier;
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
        EvaluateHovering(e);
    }

    private void EvaluateHovering(PointerEventArgs e)
    {
        var pointerPosition = e.GetCurrentPoint(this).Position;
        if (!codeCanvasContainer.Bounds.Contains(pointerPosition))
        {
            _allowedHover = false;
            RootNode.SetHoveringRecursively(false);
        }
        else
        {
            _allowedHover = true;
            RootNode.EvaluateHoveringRecursively(e);
        }
    }

    protected override Size MeasureCore(Size availableSize)
    {
        availableSize = CorrectContainedNodeWidths(availableSize);
        return base.MeasureCore(availableSize);
    }

    private Size CorrectContainedNodeWidths(Size availableSize)
    {
        // two passes to ensure that all children have sufficient width
        var requiredWidth = RootNode.CorrectContainedNodeWidths(availableSize.Width);
        RootNode.CorrectContainedNodeWidths(requiredWidth + 50);
        return availableSize.WithWidth(requiredWidth);
    }

    public bool RequestHover(SyntaxTreeListNode syntaxTreeListNode)
    {
        if (!_allowedHover)
            return false;

        return true;
    }

    public void RemoveHover(SyntaxTreeListNode listNode)
    {
        if (listNode != _hoveredNode)
            return;

        _hoveredNode = null;
        listNode.UpdateHovering(false);
    }

    public void OverrideHover(SyntaxTreeListNode listNode)
    {
        var previousHover = _hoveredNode;
        previousHover?.UpdateHovering(false);
        _hoveredNode = listNode;
        listNode.UpdateHovering(true);
    }
}
