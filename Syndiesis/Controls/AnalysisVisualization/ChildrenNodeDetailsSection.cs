using System.Collections.Generic;

namespace Syndiesis.Controls.AnalysisVisualization;

public sealed class ChildrenNodeDetailsSection : NodeDetailsSection
{
    public ChildrenNodeDetailsSection()
    {
        HeaderText = "Children";
    }

    protected override IReadOnlyList<AnalysisTreeListNode> CreateInitialNodes()
    {
        return CreateNodes(3);
    }

    public override void LoadData(NodeDetailsViewData data)
    {
        var section = data.Children;
        LoadNodes([
            section.ChildNodes,
            section.ChildTokens,
            section.ChildNodesAndTokens,
            ]);
    }
}
