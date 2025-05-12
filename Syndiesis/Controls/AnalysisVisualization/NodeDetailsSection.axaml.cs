using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Syndiesis.Core;
using Syndiesis.Core.DisplayAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Syndiesis.Controls.AnalysisVisualization;

// Do not make this abstract, the designer will not be able to load it
public partial class NodeDetailsSection : UserControl
{
    public readonly IReadOnlyList<AnalysisTreeListNode> Nodes;

    internal IAnalysisNodeHoverManager? HoverManager { get; set; }

    public string HeaderText
    {
        get => headerText.Text!;
        set => headerText.Text = value;
    }

    public NodeDetailsSection()
    {
        InitializeComponent();
        Nodes = CreateInitialNodes();
        SetNodes(Nodes);
        nodeLinePanelContainer.SetExpansionStateWithoutAnimation(ExpansionState.Expanded);
    }

    protected virtual IReadOnlyList<AnalysisTreeListNode> CreateInitialNodes()
    {
        return [];
    }

    public virtual Task LoadData(NodeDetailsViewData data)
    {
        return Task.CompletedTask;
    }

    public void SetNodes(IEnumerable<AnalysisTreeListNode> nodes)
    {
        nodeLineStackPanel.Children.ClearSetValues(nodes.ToList());
    }

    public void SetNodes(IEnumerable<UIBuilder.AnalysisTreeListNode> nodes)
    {
        var built = nodes.Select(nodes => nodes.Build());
        SetNodes(built);
    }

    public void SetNode(AnalysisTreeListNode node)
    {
        SetNodes([node]);
    }

    public void SetNode(UIBuilder.AnalysisTreeListNode node)
    {
        SetNode(node.Build());
    }

    public void SetLeftOffset(double offset)
    {
        nodeLinePanelContainer.Margin = nodeLinePanelContainer.Margin.WithLeft(-offset);
    }

    protected async Task LoadNodes(IReadOnlyList<UIBuilder.AnalysisTreeListNode> nodes)
    {
        var tasks = new Task[nodes.Count];
        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            var loadingTask = Nodes[i].SetLoading(node, node.NodeLoader);
            tasks[i] = loadingTask;
            Nodes[i].AnalysisNodeHoverManager = HoverManager;
        }
        await Task.WhenAll(tasks);
    }

    protected static IReadOnlyList<AnalysisTreeListNode> CreateNodes(int count)
    {
        var list = new List<AnalysisTreeListNode>(count);
        for (var i = 0; i < count; i++)
        {
            list.Add(CreateNode());
        }
        return list;
    }

    protected static AnalysisTreeListNode CreateNode()
    {
        return new();
    }

    private void OnOuterDisplayPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        expandToggle.Toggle();
        _ = nodeLinePanelContainer.SetExpansionState(!expandToggle.IsExpandingToggle, default);
    }

    private static readonly Color _headerGridHoverColor = Color.FromArgb(64, 128, 128, 128);
    private readonly SolidColorBrush _headerGridHoverBrush = new(Colors.Transparent);

    internal void EvaluateHovering()
    {
        var pointerOver = outerDisplayGrid.IsPointerOver;
        var color = pointerOver ? _headerGridHoverColor : Colors.Transparent;
        _headerGridHoverBrush.Color = color;
        outerDisplayGrid.Background = _headerGridHoverBrush;
    }

    public void SetMinWidth(double width)
    {
        nodeLineStackPanel.MinWidth = width;
    }

    public static NodeDetailsSection? ContainingSectionForNode(AnalysisTreeListNode? node)
    {
        if (node is null)
            return null;

        var root = node.RootNode();
        return root.NearestAncestorOfType<NodeDetailsSection>();
    }

    public void CollapseRoots()
    {
        foreach (var node in Nodes)
        {
            node.SetExpansionWithoutAnimation(false);
        }
    }
}
