using System.Collections.Generic;

namespace Syndiesis.Controls.AnalysisVisualization;

public sealed class ParentNodeDetailsSection : NodeDetailsSection
{
    public ParentNodeDetailsSection()
    {
        HeaderText = "Parent Node";
    }

    protected override IReadOnlyList<AnalysisTreeListNode> CreateInitialNodes()
    {
        return CreateNodes(1);
    }

    public override void LoadData(NodeDetailsViewData data)
    {
        var section = data.ParentNode;
        LoadNodes([
            section.ParentNode
            ]);
    }
}
