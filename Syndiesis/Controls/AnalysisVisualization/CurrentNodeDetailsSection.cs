using System.Collections.Generic;

namespace Syndiesis.Controls.AnalysisVisualization;

public sealed class CurrentNodeDetailsSection : NodeDetailsSection
{
    public AnalysisTreeListNode NodeDisplayNode => Nodes[0];
    public AnalysisTreeListNode TokenDisplayNode => Nodes[1];
    public AnalysisTreeListNode TriviaDisplayNode => Nodes[2];

    public CurrentNodeDetailsSection()
    {
        HeaderText = "Current Position";
    }

    protected override IReadOnlyList<AnalysisTreeListNode> CreateInitialNodes()
    {
        return CreateNodes(3);
    }

    public override async Task LoadData(NodeDetailsViewData data)
    {
        var section = data.CurrentNode;
        await LoadNodes([
            section.CurrentNode,
            section.CurrentToken,
            section.CurrentTrivia,
        ]);
    }
}
