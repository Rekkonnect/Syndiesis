using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Syndiesis.Controls.AnalysisVisualization;

public partial class NodeDetailsView : UserControl, IAnalysisNodeHoverManager
{
    public event Action<AnalysisTreeListNode?>? HoveredNode;
    public event Action<AnalysisTreeListNode?>? CaretHoveredNodeSet;

    public NodeDetailsView()
    {
        InitializeComponent();
        RegisterSections();
        InitializeEvents();
    }

    private void RegisterSections()
    {
        var sections = DetailsSections();
        foreach (var section in sections)
        {
            section.HoverManager = this;
        }
    }

    public async Task Load(NodeDetailsViewData viewData)
    {
        var tasks = new List<Task>();

        var sections = DetailsSections();
        foreach (var section in sections)
        {
            var task = section.LoadData(viewData);
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        SetCaretHoveredNode();
    }

    private void SetCaretHoveredNode()
    {
        CaretHoveredNodeSet?.Invoke(currentNodeSection.NodeDisplayNode);
    }

    private IReadOnlyList<NodeDetailsSection> DetailsSections()
    {
        return
        [
            currentNodeSection,
            parentSection,
            childrenSection,
            semanticModelSection,
        ];
    }

    private IEnumerable<AnalysisTreeListNode> DetailsNodes()
    {
        return DetailsSections()
            .SelectMany(s => s.Nodes)
            ;
    }

    #region Node hovers
    private bool _allowedHover = true;
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
    }

    public void RemoveHover(AnalysisTreeListNode node)
    {
        if (node != _hoveredNode)
            return;

        _hoveredNode = null;
        node.UpdateHovering(false);
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

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        EvaluateSectionHovering();
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        EvaluateSectionHovering();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        EvaluateSectionHovering();
    }

    private void EvaluateSectionHovering()
    {
        foreach (var section in DetailsSections())
        {
            section.EvaluateHovering();
        }
    }

    public AnalysisTreeListNode? NodeForShowingHoverSpan(AnalysisTreeListNode? node)
    {
        if (node is null)
            return null;

        while (true)
        {
            if (node is null)
                return null;

            if (node.NodeLine.AnalysisNodeKind is not AnalysisNodeKind.None)
            {
                break;
            }

            node = node.ParentNode;
        }

        bool shouldShow = ShouldShowHoverSpan(node);
        if (!shouldShow)
            return null;

        return node;
    }

    public bool ShouldShowHoverSpan(AnalysisTreeListNode? node)
    {
        if (node is null)
            return false;

        var section = NodeDetailsSection.ContainingSectionForNode(node);
        return section == currentNodeSection
            || section == parentSection
            || section == childrenSection
            ;
    }

    #region Scrolls
    private const double extraScrollHeight = 50;
    private const double extraScrollWidth = 20;

    private bool _isUpdatingScrollLimits = false;

    private double _topOffset;
    private double _leftOffset;

    private void UpdateScrollLimits()
    {
        // Probably hacked together
        var maxWidth = RequiredWidth();
        var childrenHeight = contentPanel.Children.Sum(s => s.Bounds.Height);

        _isUpdatingScrollLimits = true;

        using (verticalScrollBar.BeginUpdateBlock())
        {
            var height = childrenHeight;
            verticalScrollBar.MaxValue = height + extraScrollHeight;
            verticalScrollBar.StartPosition = _topOffset;
            verticalScrollBar.EndPosition = contentPanel.Bounds.Height;
            verticalScrollBar.SetAvailableScrollOnScrollableWindow();
        }

        using (horizontalScrollBar.BeginUpdateBlock())
        {
            horizontalScrollBar.MaxValue = Math.Max(maxWidth - extraScrollWidth, 0);
            horizontalScrollBar.StartPosition = _leftOffset;
            horizontalScrollBar.EndPosition = _leftOffset + contentPanel.Bounds.Width;
            horizontalScrollBar.SetAvailableScrollOnScrollableWindow();
        }

        _isUpdatingScrollLimits = false;
    }

    private double RequiredWidth()
    {
        var nodes = DetailsNodes();
        return nodes.Max(s => s.Bounds.Width);
    }

    private void InitializeEvents()
    {
        verticalScrollBar.ScrollChanged += OnVerticalScroll;
        horizontalScrollBar.ScrollChanged += OnHorizontalScroll;

        foreach (var section in DetailsSections())
        {
            section.nodeLinePanelContainer.SizeChanged += HandleContentSizeChanged;
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
    }

    private void HandleContentSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        InvalidateMeasure();
    }

    private void OnVerticalScroll()
    {
        if (_isUpdatingScrollLimits)
            return;

        var top = verticalScrollBar.StartPosition;
        _topOffset = top;
        contentPanel.Margin = contentPanel.Margin.WithTop(-top);
        InvalidateArrange();
    }

    private void OnHorizontalScroll()
    {
        if (_isUpdatingScrollLimits)
            return;

        var left = horizontalScrollBar.StartPosition;
        SetLeftOffset(left);

        InvalidateArrange();
    }

    private void SetLeftOffset(double left)
    {
        _leftOffset = left;
        foreach (var section in DetailsSections())
        {
            section.SetLeftOffset(left);
        }
    }

    #endregion

    private void CorrectPositionFromHorizontalScroll(Size availableSize)
    {
        var offset = _leftOffset;
        double requiredWidth = RequiredWidth();
        // ensure that the width has been initialized
        if (requiredWidth is not > 0)
            return;

        var availableRight = requiredWidth - offset;
        var scrollBarWidth = verticalScrollBar.Bounds.Width;
        var missing = availableSize.Width - availableRight - scrollBarWidth;
        if (missing > 0)
        {
            var reducedOffset = offset - missing;
            var targetOffset = Math.Max(reducedOffset, 0);
            SetLeftOffset(targetOffset);
        }
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

    protected override Size MeasureCore(Size availableSize)
    {
        double requiredWidth = RequiredWidth();
        if (requiredWidth is not double.PositiveInfinity)
        {
            requiredWidth = Math.Max(availableSize.Width, requiredWidth);
            foreach (var section in DetailsSections())
            {
                section.SetMinWidth(requiredWidth);
            }
        }
        CorrectPositionFromHorizontalScroll(availableSize);
        return base.MeasureCore(availableSize);
    }
}
