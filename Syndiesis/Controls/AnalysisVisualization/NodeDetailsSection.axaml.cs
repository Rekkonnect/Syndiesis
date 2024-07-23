using Avalonia.Controls;
using Avalonia.Input;
using Syndiesis.Core;
using Syndiesis.Core.DisplayAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Syndiesis.Controls.AnalysisVisualization;

// Do not make this abstract, the designer will not be able to load it
public partial class NodeDetailsSection : UserControl
{
    protected readonly IReadOnlyList<AnalysisTreeListNode> Nodes;

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

    public virtual void LoadData(NodeDetailsViewData data) { }

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

    protected void LoadNodes(IReadOnlyList<UIBuilder.AnalysisTreeListNode> nodes)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            _ = Nodes[i].SetLoading(node, node.NodeLoader);
            Nodes[i].AnalysisNodeHoverManager = HoverManager;
        }
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
}
