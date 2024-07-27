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
using System.Linq;
using System.Threading.Tasks;

namespace Syndiesis.Controls.AnalysisVisualization;

using NodeBuilderChildren = IReadOnlyList<UIBuilder.AnalysisTreeListNode>;
using NodeChildren = IReadOnlyList<AnalysisTreeListNode>;

public partial class AnalysisTreeListNode : UserControl
{
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
            HasChildren = _childRetriever is not null;
            value.PointerEntered += OnPointerEnteredLine;
        }
    }

    // Only here for the designer preview
    public AvaloniaList<AnalysisTreeListNode> ChildNodes
    {
        set
        {
            SetChildNodes(value);
            HasChildren = value.Count > 0;
        }
    }

    public bool HasChildren
    {
        get
        {
            return NodeLine.HasChildren;
        }
        set
        {
            NodeLine.HasChildren = value;
            if (!value)
            {
                SetExpansionWithoutAnimation(false);
            }
        }
    }

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
                SetLoadingNode();
                if (NodeLine.IsExpanded)
                {
                    _ = RequestInitializedChildren();
                }
            }

            HasChildren = value is not null;
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

    internal IAnalysisNodeHoverManager? AnalysisNodeHoverManager { get; set; }

    public AnalysisTreeListNode? ParentNode
    {
        get
        {
            return this.NearestAncestorOfType<AnalysisTreeListNode>();
        }
    }

    public AnalysisTreeListNode()
    {
        InitializeComponent();
    }

    public AnalysisTreeListNode RootNode()
    {
        var current = this;
        while (true)
        {
            var next = current.ParentNode;
            if (next is null)
                return current;

            current = next;
        }
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

        _childRetrievalTask = ChildRetrievalTaskWorker();
        await _childRetrievalTask;
        _childRetrievalTask = null;
    }

    private async Task ChildRetrievalTaskWorker()
    {
        var result = await Task.Run(_childRetriever!.GetValueAsync);
        await SetLoadedChildren(result);
    }

    private async Task SetLoadedChildren(NodeBuilderChildren? builders)
    {
        if (builders is null)
            return;

        var estimator = new DynamicIterationEstimator(TimeSpan.FromMilliseconds(10));
        int start = 0;
        var loadedList = new List<AnalysisTreeListNode>();
        _loadedChildren = loadedList;

        async Task UIUpdate()
        {
            if (start >= builders.Count)
            {
                loadingNode.IsVisible = false;
                return;
            }

            int chunk = estimator.RecommendedIterationCount;
            if (chunk is 0)
            {
                // For the first chunk, a little bit of lag by overshooting the device's
                // capabilities is okay
                chunk = 20;
            }

            if (chunk <= 5)
            {
                LoggerExtensionsEx.LogLowEndDevice();
            }

            var taken = builders.Skip(start).Take(chunk);
            start += chunk;

            using (estimator.BeginProcess(chunk))
            {
                var children = taken.Select(s => s.Build()).ToList();
                loadedList.AddRange(children);

                innerStackPanel.Children.AddRange(children);
                foreach (var child in children)
                {
                    child.AnalysisNodeHoverManager = AnalysisNodeHoverManager;
                }
            }

            loadingNode.SetProgress(new(start, builders.Count));

            await Task.Delay(40);
            await Dispatcher.UIThread.InvokeAsync(UIUpdate);
        }

        await Dispatcher.UIThread.InvokeAsync(UIUpdate);
    }

    private void SetChildNodes(NodeChildren value)
    {
        innerStackPanel.Children.ClearSetValues(value);
    }

    private void OnPointerEnteredLine(object? sender, PointerEventArgs e)
    {
        SetAsHovered();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        AnalysisNodeHoverManager?.RemoveHover(this);
        base.OnPointerExited(e);
    }

    private void EvaluateHovering(PointerEventArgs e)
    {
        var allowedHover = AnalysisNodeHoverManager?.RequestHover(this) ?? true;
        if (!allowedHover)
        {
            UpdateHovering(false);
            return;
        }

        var nodeLine = NodeLine;
        var bounds = nodeLine.Bounds;
        var position = e.GetCurrentPoint(nodeLine).Position;
        var isHovered = bounds.Contains(position);

        if (isHovered)
        {
            AnalysisNodeHoverManager?.OverrideHover(this);
            _ = RequestInitializedChildren();
        }
        else
        {
            AnalysisNodeHoverManager?.RemoveHover(this);
        }
    }

    private void SetAsHovered()
    {
        var allowedHover = AnalysisNodeHoverManager?.RequestHover(this) ?? true;
        if (!allowedHover)
        {
            UpdateHovering(false);
            return;
        }

        AnalysisNodeHoverManager?.OverrideHover(this);
        _ = RequestInitializedChildren();
    }

    internal void SetListViewRecursively(AnalysisTreeListView listView)
    {
        AnalysisNodeHoverManager = listView;

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

    internal void EvaluateHoveringRecursivelyBinary(PointerEventArgs e)
    {
        EvaluateHovering(e);
        var position = e.GetPosition(this);

        int index = BinarySearchControlByTop(position.Y);
        if (index < 0)
            return;

        var children = LazyChildren;
        var child = LazyChildren[index];
        child.EvaluateHoveringRecursivelyBinary(e);
    }

    private int BinarySearchControlByTop(double height)
    {
        var children = LazyChildren;

        int low = 0;
        int high = children.Count - 1;

        while (low <= high)
        {
            int mid = (low + high) / 2;
            var child = LazyChildren[mid];
            var childBounds = child.Bounds;
            if (childBounds.Top <= height && height <= childBounds.Bottom)
                return mid;

            if (childBounds.Top < height)
                low = mid + 1;
            else
                high = mid - 1;
        }

        return -1;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        EvaluateHovering(e);

        var modifiers = e.KeyModifiers.NormalizeByPlatform();
        var properties = e.GetCurrentPoint(this).Properties;
        if (properties.IsLeftButtonPressed)
        {
            if (AnalysisNodeHoverManager?.IsHovered(this) is not true)
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

    public void DestroyLoadedChildren()
    {
        _loadedChildren = null;
        _childRetriever?.ClearValue();
        SetLoadingNode();
    }

    private void SetLoadingNode()
    {
        loadingNode.IsVisible = true;
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

        _ = RequestInitializedChildren(expand);
        _ = expandableCanvas.SetExpansionState(expand, default);
    }

    public async Task SetLoading(
        UIBuilder.AnalysisTreeListNode loadingAppearance,
        Task<UIBuilder.AnalysisTreeListNode?>? builderTask)
    {
        SetLoadingState(loadingAppearance);
        await LoadFromTask(builderTask);
    }

    public void SetLoadingState(UIBuilder.AnalysisTreeListNode loadingAppearance)
    {
        innerStackPanel.Children.Clear();
        _loadedChildren = null;
        ReloadFromBuilder(loadingAppearance);
    }

    /// <remarks>
    /// Always invoke this from the UI thread. The builder task may be a task
    /// executing on any thread.
    /// </remarks>
    public async Task LoadFromTask(Task<UIBuilder.AnalysisTreeListNode?>? builderTask)
    {
        if (builderTask is null)
            return;

        var builder = await builderTask;
        ReloadFromBuilder(builder);
    }

    /// <remarks>
    /// Always invoke this from the UI thread.
    /// </remarks>
    public void ReloadFromBuilder(UIBuilder.AnalysisTreeListNode? builder)
    {
        if (builder is null)
            return;

        // for initialization purposes; avoid this hack in the future
        if (NodeLine.Bounds.Height is 0)
        {
            NodeLine = builder.NodeLine.Build();
        }
        else
        {
            NodeLine.ReloadFromBuilder(builder.NodeLine);
        }

        ChildRetriever = builder.ChildRetriever;
        AssociatedSyntaxObject = builder.AssociatedSyntaxObject;
    }
}
