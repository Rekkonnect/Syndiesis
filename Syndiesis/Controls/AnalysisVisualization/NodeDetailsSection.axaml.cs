using Avalonia.Controls;
using Syndiesis.Core.DisplayAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Syndiesis.Controls.AnalysisVisualization;

public partial class NodeDetailsSection : UserControl
{
    public string HeaderText
    {
        get => headerText.Text!;
        set => headerText.Text = value;
    }

    public NodeDetailsSection()
    {
        InitializeComponent();
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
}
