using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Garyon.Extensions;
using Syndiesis.Core;
using Syndiesis.Core.DisplayAnalysis;
using Syndiesis.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Syndiesis.Controls.AnalysisVisualization;

using NodeChildren = IReadOnlyList<AnalysisTreeListNode>;
using NodeBuilderChildren = IReadOnlyList<UIBuilder.AnalysisTreeListNode>;

public partial class AnalysisTreeListNode : UserControl
{
    private static readonly CancellationTokenFactory _expansionAnimationCancellationTokenFactory = new();

    public object? AssociatedSyntaxObjectContent
    {
        get => AssociatedSyntaxObject?.SyntaxObject;
        set
        {
            var o = SyntaxObjectInfo.GetInfoForObject(value);
            AssociatedSyntaxObject = o;
        }
    }

    public SyntaxObjectInfo? AssociatedSyntaxObject
    {
        get => NodeLine.AssociatedSyntaxObject;
        set => NodeLine.AssociatedSyntaxObject = value;
    }

    private AnalysisTreeListNodeLine _nodeLine = new();

    public AnalysisTreeListNodeLine NodeLine
    {
        get => _nodeLine;
        set
        {
            _nodeLine = value;
            topNodeContent.Content = value;
            value.HasChildren = _childRetriever is not null;
        }
    }

    // Only here for the designer preview
    public AvaloniaList<AnalysisTreeListNode> ChildNodes
    {
        set
        {
            SetChildNodes(value);
            NodeLine.HasChildren = value.Count > 0;
        }
    }

    public bool HasChildren => NodeLine.HasChildren;

    private AdvancedLazy<NodeBuilderChildren>? _childRetriever;
    private Task? _childRetrievalTask;
    private NodeChildren? _loadedChildren;

    public Task? ChildRetrievalTask => _childRetrievalTask;

    public AnalysisNodeChildRetriever? ChildRetriever
    {
        set
        {
            if (value is null)
            {
                _childRetriever = null;
                innerStackPanel.Children.Clear();
            }
            else
            {
                // if only delegates could be converted more seamlessly
                _childRetriever = new(new Func<NodeBuilderChildren>(value));
                innerStackPanel.Children.ClearSetValue(new LoadingTreeListNode());
            }

            NodeLine.HasChildren = value is not null;
        }
    }

    public NodeChildren LazyChildren
    {
        get
        {
            return _loadedChildren ?? [];
        }
    }

    public bool HasLoadedChildren => _childRetriever?.IsValueCreated ?? true;

    internal AnalysisTreeListView? ListView { get; set; }

    public AnalysisTreeListNode? ParentNode
    {
        get
        {
            StyledElement? current = this;
            while (current is not null)
            {
                var parent = current.Parent;
                if (parent is AnalysisTreeListNode node)
                    return node;

                current = parent;
            }

            return null;
        }
    }

    public AnalysisTreeListNode()
    {
        InitializeComponent();
    }

    private static readonly Color _topLineHoverColor = Color.FromArgb(64, 128, 128, 128);
    private static readonly Color _expandableCanvasHoverColor = Color.FromArgb(64, 96, 96, 96);

    private readonly SolidColorBrush _topLineBackgroundBrush = new(Colors.Transparent);
    private readonly SolidColorBrush _expandableCanvasBackgroundBrush = new(Colors.Transparent);

    public async Task RequestInitializedChildren(bool flag)
    {
        if (flag)
        {
            await RequestInitializedChildren();
        }
    }

    public async Task RequestInitializedChildren()
    {
        if (_childRetriever is null)
            return;

        if (_childRetriever.IsValueCreated)
            return;

        var retrievalTask = Task.Run(_childRetriever.GetValueAsync);
        _childRetrievalTask = retrievalTask;
        var result = await retrievalTask;
        _childRetrievalTask = null;
        SetLoadedChildren(result);
    }

    private void SetLoadedChildren(NodeBuilderChildren builders)
    {
        void UIUpdate()
        {
            builders ??= [];
            var children = builders.Select(s => s.Build()).ToList();
            _loadedChildren = children;

            SetChildNodes(children);
            foreach (var child in children)
            {
                child.ListView = ListView;
            }
        }

        Dispatcher.UIThread.Invoke(UIUpdate);
    }

    private void SetChildNodes(NodeChildren value)
    {
        innerStackPanel.Children.ClearSetValues(value);
        //_ = expandableCanvas.AnimateCurrentHeight(default);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        EvaluateHoveringRecursively(e);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        EvaluateHovering(e);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        EvaluateHoveringRecursively(e);
    }

    private void EvaluateHovering(PointerEventArgs e)
    {
        var allowedHover = ListView?.RequestHover(this) ?? true;
        if (!allowedHover)
        {
            UpdateHovering(false);
            return;
        }

        var nodeLine = NodeLine;
        var bounds = nodeLine.Bounds;
        var restrictedBounds = bounds
            .WithHeight(bounds.Height - 1);
        var position = e.GetCurrentPoint(nodeLine).Position;
        var isHovered = restrictedBounds.Contains(position);

        if (isHovered)
        {
            ListView?.OverrideHover(this);
            _ = RequestInitializedChildren();
        }
        else
        {
            ListView?.RemoveHover(this);
        }
    }

    internal void SetListViewRecursively(AnalysisTreeListView listView)
    {
        ListView = listView;

        foreach (var child in LazyChildren)
        {
            child.SetListViewRecursively(listView);
        }
    }

    internal void SetHoveringRecursively(bool isHovered)
    {
        UpdateHovering(isHovered);

        foreach (var child in LazyChildren)
        {
            child.UpdateHovering(isHovered);
        }
    }

    internal void UpdateHovering(bool isHovered)
    {
        var topLineBackgroundColor = isHovered ? _topLineHoverColor : Colors.Transparent;
        var expandableCanvasBackgroundColor = isHovered
            ? _expandableCanvasHoverColor
            : Colors.Transparent;
        _topLineBackgroundBrush.Color = topLineBackgroundColor;
        _expandableCanvasBackgroundBrush.Color = expandableCanvasBackgroundColor;

        NodeLine.Background = _topLineBackgroundBrush;
        expandableCanvas.Background = _expandableCanvasBackgroundBrush;
    }

    internal void EvaluateHoveringRecursively(PointerEventArgs e)
    {
        EvaluateHovering(e);

        foreach (var child in LazyChildren)
        {
            child.EvaluateHoveringRecursively(e);
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        EvaluateHovering(e);

        var modifiers = e.KeyModifiers.NormalizeByPlatform();
        var properties = e.GetCurrentPoint(this).Properties;
        if (properties.IsLeftButtonPressed)
        {
            if (ListView?.IsHovered(this) is not true)
                return;

            switch (modifiers)
            {
                case KeyModifiers.None:
                    ToggleExpansion();
                    break;

                case KeyModifiers.Alt:
                    ExpandRecursively();
                    break;
            }
        }
    }

    private void ExpandRecursively()
    {
        int depth = AppSettings.Instance.RecursiveExpansionDepth;
        Task.Run(() => ExpandRecursivelyAsync(depth));
    }

    public async Task ExpandRecursivelyAsync(int depth)
    {
        if (depth <= 0)
            return;

        Dispatcher.UIThread.Invoke(Expand);
        await RequestInitializedChildren();
        var taskList = new List<Task>();
        foreach (var node in LazyChildren)
        {
            taskList.Add(node.ExpandRecursivelyAsync(depth - 1));
        }
        await taskList.WaitAll();
    }

    public void ToggleExpansion()
    {
        var nodeLine = NodeLine;
        if (!nodeLine.HasChildren)
            return;

        bool newToggle = !nodeLine.IsExpanded;
        ExpandOrCollapse(newToggle);
    }

    public void SetExpansionWithoutAnimationRecursively(bool expand, int depth = int.MaxValue)
    {
        if (depth <= 0)
            return;

        _ = RequestInitializedChildren(expand);
        foreach (var node in LazyChildren)
        {
            node.SetExpansionWithoutAnimationRecursively(expand, depth - 1);
        }
        SetExpansionWithoutAnimation(expand);
    }

    public void SetExpansionWithoutAnimation(bool expand)
    {
        _ = RequestInitializedChildren(expand);
        var state = expand ? ExpansionState.Expanded : ExpansionState.Collapsed;
        NodeLine.IsExpanded = expand;
        expandableCanvas.SetExpansionStateWithoutAnimation(state);
    }

    public void Expand()
    {
        ExpandOrCollapse(true);
    }

    public void Collapse()
    {
        ExpandOrCollapse(false);
    }

    private void ExpandOrCollapse(bool expand)
    {
        var nodeLine = NodeLine;
        if (!nodeLine.HasChildren)
            return;

        if (nodeLine.IsExpanded == expand)
            return;

        nodeLine.IsExpanded = expand;

        // cancel any currently running animation
        _expansionAnimationCancellationTokenFactory.Cancel();

        var animationToken = _expansionAnimationCancellationTokenFactory.CurrentToken;
        _ = RequestInitializedChildren(expand);
        _ = expandableCanvas.SetExpansionState(expand, animationToken);
    }
}
