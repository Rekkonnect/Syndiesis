using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Syndiesis.Utilities;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Syndiesis.Controls.AnalysisVisualization;

public partial class AnalysisTreeListView : UserControl, IAnalysisNodeHoverManager
{
    private const double extraScrollHeight = 50;
    private const double extraScrollWidth = 20;

    private AnalysisTreeListNode _rootNode = new();

    public AnalysisTreeListNode RootNode
    {
        get => _rootNode;
        set
        {
            _rootNode = value;
            topLevelNodeContent.Content = value;

            value.SetListViewRecursively(this);
            value.SizeChanged += HandleRootNodeSizeAdjusted;
            AnalyzedTree = value.AssociatedSyntaxObject?.SyntaxTree;
            UpdateRootChanged();
        }
    }

    public SyntaxTree? AnalyzedTree { get; set; }

    public AnalysisNodeKind TargetAnalysisNodeKind { get; set; }

    public AnalysisTreeListNode? CurrentHoveredNode => _hoveredNode;

    public event Action<AnalysisTreeListNode?>? HoveredNode;
    public event Action<AnalysisTreeListNode>? RequestedPlaceCursorAtNode;
    public event Action<AnalysisTreeListNode>? RequestedSelectTextAtNode;
    public event Action? NewRootLoaded;

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
        ResetToInitialRootView();
        NewRootLoaded?.Invoke();
    }

    public AnalysisTreeListView()
    {
        InitializeComponent();
        InitializeEvents();
    }

    #region Scrolls

    private bool _isUpdatingScrollLimits = false;

    private void UpdateScrollLimits()
    {
        var node = RootNode;

        _isUpdatingScrollLimits = true;

        using (verticalScrollBar.BeginUpdateBlock())
        {
            verticalScrollBar.MaxValue = node.Bounds.Height + extraScrollHeight;
            verticalScrollBar.StartPosition = -Canvas.GetTop(topLevelNodeContent);
            verticalScrollBar.EndPosition = verticalScrollBar.StartPosition + contentCanvas.Bounds.Height;
            verticalScrollBar.SetAvailableScrollOnScrollableWindow();
        }

        using (horizontalScrollBar.BeginUpdateBlock())
        {
            horizontalScrollBar.MaxValue = Math.Max(node.Bounds.Width - extraScrollWidth, 0);
            horizontalScrollBar.StartPosition = -Canvas.GetLeft(topLevelNodeContent);
            horizontalScrollBar.EndPosition = horizontalScrollBar.StartPosition + contentCanvas.Bounds.Width;
            horizontalScrollBar.SetAvailableScrollOnScrollableWindow();
        }

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

    #endregion

    public void ResetToInitialRootView()
    {
        RootNode.SetExpansionWithoutAnimationRecursively(false);
        RootNode.Expand();
        DestroyChildrenFromSecondLevel();
        horizontalScrollBar.StartPosition = 0;
        verticalScrollBar.StartPosition = 0;
    }

    private void DestroyChildrenFromSecondLevel()
    {
        foreach (var child in RootNode.LazyChildren)
        {
            child.DestroyLoadedChildren();
        }
    }

    public AnalysisTreeListNode? DiscoverParentNodeCoveringSelection(int start, int end)
    {
        var span = TextSpan.FromBounds(start, end);
        var startNode = GetNodeAtPosition(span)?.ParentNode;

        if (startNode is null)
            return null;

        var current = startNode;
        var targetKind = TargetAnalysisNodeKind;
        while (true)
        {
            var parentNode = current.ParentNode;
            if (parentNode is null)
                return current;

            if (current.NodeLine.AnalysisNodeKind != targetKind)
            {
                goto next;
            }

            var displaySpan = parentNode.NodeLine.DisplaySpan;
            bool contained = displaySpan.Contains(start)
                && displaySpan.Contains(end);

            if (contained)
            {
                return parentNode;
            }

        next:
            current = parentNode;
        }
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

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var properties = e.GetCurrentPoint(this).Properties;
        if (properties.IsRightButtonPressed)
        {
            var modifiers = e.KeyModifiers.NormalizeByPlatform();
            switch (modifiers)
            {
                case KeyModifiers.Control:
                    if (_hoveredNode is null)
                        break;

                    RequestedPlaceCursorAtNode?.Invoke(_hoveredNode);
                    break;

                case KeyModifiers.Control | KeyModifiers.Shift:
                    if (_hoveredNode is null)
                        break;

                    RequestedSelectTextAtNode?.Invoke(_hoveredNode);
                    break;
            }
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        const double scrollMultiplier = 50;
        ScrollingHelpers.ApplyWheelScrolling(
            e,
            scrollMultiplier,
            verticalScrollBar,
            horizontalScrollBar);

        EvaluateHovering(e);
    }

    public void EvaluateHovering(PointerEventArgs e)
    {
        var pointerPosition = e.GetCurrentPoint(contentCanvasContainer).Position;
        if (!contentCanvasContainer.Bounds.Contains(pointerPosition))
        {
            _allowedHover = false;
            RootNode.SetHoveringRecursively(false);
        }
        else
        {
            var previous = _allowedHover;
            _allowedHover = true;
            if (!previous)
            {
                RootNode.EvaluateHoveringRecursivelyBinary(e);
            }
        }
    }

    protected override Size MeasureCore(Size availableSize)
    {
        availableSize = CorrectContainedNodeWidths(availableSize);
        return base.MeasureCore(availableSize);
    }

    protected override void ArrangeCore(Rect finalRect)
    {
        base.ArrangeCore(finalRect);
        UpdateScrollLimits();
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        CorrectPositionFromHorizontalScroll(finalSize);
        return base.ArrangeOverride(finalSize);
    }

    private void CorrectPositionFromHorizontalScroll(Size availableSize)
    {
        var offset = -Canvas.GetLeft(topLevelNodeContent);
        var rootWidth = topLevelNodeContent.Bounds.Width;
        // ensure that the width has been initialized
        if (rootWidth is not > 0)
            return;

        var availableRight = rootWidth - offset;
        var scrollBarWidth = verticalScrollBar.Bounds.Width;
        var missing = availableSize.Width - availableRight - scrollBarWidth;
        if (missing > 0)
        {
            var reducedOffset = offset - missing;
            var targetOffset = Math.Min(-reducedOffset, 0);
            Canvas.SetLeft(topLevelNodeContent, targetOffset);
        }
    }

    private Size CorrectContainedNodeWidths(Size availableSize)
    {
        topLevelNodeContent.MinWidth = availableSize.Width;
        CorrectPositionFromHorizontalScroll(availableSize);
        return availableSize;
    }

    #region Node hovers
    private bool _allowedHover;
    private AnalysisTreeListNode? _hoveredNode;

    public bool RequestHover(AnalysisTreeListNode node)
    {
        if (!_allowedHover)
            return false;

        var parent = node.ParentNode;
        if (parent is null)
            return true;

        return parent.NodeLine.IsExpanded;
    }

    public bool IsHovered(AnalysisTreeListNode node)
    {
        return _hoveredNode == node;
    }

    public void ClearHover()
    {
        _hoveredNode?.UpdateHovering(false);
        _hoveredNode = null;
        HoveredNode?.Invoke(null);
    }

    public void RemoveHover(AnalysisTreeListNode node)
    {
        if (node != _hoveredNode)
            return;

        _hoveredNode = null;
        node.UpdateHovering(false);
        HoveredNode?.Invoke(null);
    }

    public void OverrideHover(AnalysisTreeListNode node)
    {
        if (_hoveredNode == node)
            return;

        var previousHover = _hoveredNode;
        previousHover?.UpdateHovering(false);
        _hoveredNode = node;
        node.UpdateHovering(true);
        HoveredNode?.Invoke(node);
    }
    #endregion

    // Initialize to an impossible value to trigger
    private TextSpan _recurringSpanExpansion = new(0, int.MaxValue);

    public event Action<AnalysisTreeListNode?>? CaretHoveredNodeSet;

    public async Task EnsureHighlightedPositionRecurring(TextSpan span)
    {
        _recurringSpanExpansion = span;

        var previousNode = RootNode;

        await AwaitExpansion(previousNode);

        while (true)
        {
            var node = GetTargetKindNodeAtPosition(span, previousNode);

            if (node is null)
                return;

            OverrideHover(node);
            BringToView(node);
            CaretHoveredNodeSet?.Invoke(node);

            if (node == previousNode)
                return;

            previousNode = node;

            if (!node.HasChildren)
                return;

            await AwaitExpansion(node);

            if (_recurringSpanExpansion != span)
                return;
        }
    }

    private async Task AwaitExpansion(AnalysisTreeListNode node)
    {
        node.Expand();
        var retrievalTask = node.ChildRetrievalTask;
        if (retrievalTask is not null)
        {
            await retrievalTask;
        }
    }

    private AnalysisTreeListNode? GetTargetKindNodeAtPosition(TextSpan span, AnalysisTreeListNode root)
    {
        var node = GetNodeAtPosition(span, root);
        if (node is null)
            return null;

        var current = node;
        while (true)
        {
            if (current is null)
                return null;

            if (current.NodeLine.AnalysisNodeKind == TargetAnalysisNodeKind)
            {
                return current;
            }

            current = current.ParentNode;
        }
    }

    private AnalysisTreeListNode? GetNodeAtPosition(TextSpan span)
    {
        return GetNodeAtPosition(span, RootNode);
    }

    private AnalysisTreeListNode? GetNodeAtPosition(TextSpan span, AnalysisTreeListNode root)
    {
        if (AnalyzedTree is null)
            return null;

        var analyzedLength = AnalyzedTree.Length;
        if (analyzedLength > 0)
        {
            if (span.Start >= analyzedLength)
            {
                span = new(analyzedLength - 1, 0);
            }

            if (span.End >= analyzedLength)
            {
                span = TextSpan.FromBounds(span.Start, analyzedLength);
            }
        }

        var current = root;
        while (true)
        {
            if (!current.HasChildren)
            {
                return current;
            }

            if (!current.HasLoadedChildren)
            {
                return current;
            }

            var children = ExpandLazyChildren(current);
            var relevantChildren = children
                .Where(s =>
                {
                    var nodeLine = s.NodeLine;
                    return nodeLine.AnalysisNodeKind == TargetAnalysisNodeKind
                        && nodeLine.DisplaySpan.Contains(span);
                })
                .ToArray();

            // if no position corresponds to our tree, the position is within the node
            // but is not displayed in our tree (for example because trivia is hidden)
            if (relevantChildren is [])
            {
                return current;
            }

            // resolve the most suitable child by the shortest span that it contains
            // ties are not resolved
            var child = relevantChildren.MinBy(s => s.NodeLine.DisplaySpan.Length)!;
            current = child;
        }
    }

    private AnalysisTreeListNode GetLastLoadedNode()
    {
        var current = RootNode;
        Debug.Assert(current is not null);
        while (true)
        {
        start:
            var children = ExpandLazyChildren(current);
            if (children is [])
                return current;

            for (int i = children.Count - 1; i >= 0; i--)
            {
                var child = children[i];
                if (child.NodeLine.AnalysisNodeKind != TargetAnalysisNodeKind)
                {
                    continue;
                }

                current = child;
                goto start;
            }

            return current;
        }
    }

    private IReadOnlyList<AnalysisTreeListNode> ExpandLazyChildren(AnalysisTreeListNode node)
    {
        node.SetExpansionWithoutAnimation(true);
        return node.LazyChildren;
    }

    public void BringToView(AnalysisTreeListNode node)
    {
        var basis = contentCanvasContainer;
        var translation = node.TranslatePoint(default, basis);
        if (translation is null)
        {
            node.Loaded += (_, _) => BringToView(node);
            return;
        }

        var point = translation.Value;
        var offset = topLevelNodeContent.TranslatePoint(default, basis).GetValueOrDefault();
        var leftOffset = offset.X;
        var topOffset = offset.Y;
        var x = point.X - leftOffset - 150;
        var y = point.Y - topOffset - 150;
        horizontalScrollBar.SetStartPositionPreserveLength(x);
        verticalScrollBar.SetStartPositionPreserveLength(y);
    }
}
