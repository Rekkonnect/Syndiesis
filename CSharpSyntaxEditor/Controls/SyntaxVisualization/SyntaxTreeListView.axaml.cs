using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;

namespace CSharpSyntaxEditor.Controls;

/*
 * WARNING: We currently get this error a lot:
 * [Layout]Layout cycle detected. Item 'CSharpSyntaxEditor.Controls.SyntaxTreeListView' was enqueued '10' times.(LayoutQueue`1 #54044048)
 * And we must do something about it
 */

public partial class SyntaxTreeListView : UserControl
{
    const double extraScrollHeight = 50;
    const double extraScrollWidth = 40;

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

    private bool _isUpdatingScrollLimits = false;

    private void UpdateScrollLimits()
    {
        var node = RootNode;

        _isUpdatingScrollLimits = true;

        verticalScrollBar.BeginUpdate();
        verticalScrollBar.MaxValue = node.Bounds.Height + extraScrollHeight;
        verticalScrollBar.StartPosition = -Canvas.GetTop(topLevelNodeContent);
        verticalScrollBar.EndPosition = verticalScrollBar.StartPosition + codeCanvas.Bounds.Height;
        verticalScrollBar.SetAvailableScrollOnScrollableWindow();
        verticalScrollBar.EndUpdate();

        horizontalScrollBar.BeginUpdate();
        horizontalScrollBar.MaxValue = Math.Max(node.Bounds.Width - 10, 0);
        horizontalScrollBar.StartPosition = -Canvas.GetLeft(topLevelNodeContent);
        horizontalScrollBar.EndPosition = horizontalScrollBar.StartPosition + codeCanvas.Bounds.Width;
        horizontalScrollBar.SetAvailableScrollOnScrollableWindow();
        horizontalScrollBar.EndUpdate();

        _isUpdatingScrollLimits = false;
    }

    private void InitializeEvents()
    {
        verticalScrollBar.ScrollChanged += OnVerticalScroll;
        horizontalScrollBar.ScrollChanged += OnHorizontalScroll;
    }

    private void OnVerticalScroll()
    {
        if (_isUpdatingScrollLimits)
            return;

        var top = verticalScrollBar.StartPosition;
        Canvas.SetTop(topLevelNodeContent, -top);
        InvalidateArrange();
    }

    private void OnHorizontalScroll()
    {
        if (_isUpdatingScrollLimits)
            return;

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

    protected override Size ArrangeOverride(Size finalSize)
    {
        CorrectPositionFromHorizontalScroll(finalSize);
        return base.ArrangeOverride(finalSize);
    }

    private void CorrectPositionFromHorizontalScroll(Size availableSize)
    {
        var offset = -Canvas.GetLeft(topLevelNodeContent);
        var rootWidth = RootNode.Width;
        // ensure that the width has been initialized
        if (rootWidth is not > 0)
            return;

        var availableRight = rootWidth - offset;
        var missing = availableSize.Width - availableRight;
        if (missing > 0)
        {
            var reducedOffset = offset - missing;
            Canvas.SetLeft(topLevelNodeContent, -reducedOffset);
        }
    }

    private Size CorrectContainedNodeWidths(Size availableSize)
    {
        // two passes to ensure that all children have sufficient width
        var previousWidth = RootNode.Width;
        var availableWidth = availableSize.Width;
        var requiredWidth = RootNode.CorrectContainedNodeWidths(availableWidth);
        if (previousWidth is not > 0 || requiredWidth - 1 >= previousWidth)
        {
            // only resize if we are not long enough 8)
            // PROBLEM: this causes an awkward scroll behavior where the sudden
            // increase in the width is shown in the scroll bar in the form of length jumps
            RootNode.CorrectContainedNodeWidths(requiredWidth + extraScrollWidth + 10);
        }
        return availableSize.WithWidth(requiredWidth);
    }

    #region Node hovers
    public bool RequestHover(SyntaxTreeListNode node)
    {
        if (!_allowedHover)
            return false;

        return true;
    }

    public bool IsHovered(SyntaxTreeListNode node)
    {
        return _hoveredNode == node;
    }

    public void RemoveHover(SyntaxTreeListNode node)
    {
        if (node != _hoveredNode)
            return;

        _hoveredNode = null;
        node.UpdateHovering(false);
    }

    public void OverrideHover(SyntaxTreeListNode node)
    {
        var previousHover = _hoveredNode;
        previousHover?.UpdateHovering(false);
        _hoveredNode = node;
        node.UpdateHovering(true);
    }
    #endregion
}
