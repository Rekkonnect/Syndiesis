using System.Collections.Generic;

namespace Syndiesis.Controls.AnalysisVisualization;

public sealed class CurrentNodeDetailsSection : NodeDetailsSection
{
    public CurrentNodeDetailsSection()
    {
        HeaderText = "Current Node";
    }

    protected override IReadOnlyList<AnalysisTreeListNode> CreateInitialNodes()
    {
        return CreateNodes(1);
    }

    public override void LoadData(NodeDetailsViewData data)
    {
        var section = data.CurrentNode;
        LoadNodes([
            section.CurrentNode
            ]);
    }
}
