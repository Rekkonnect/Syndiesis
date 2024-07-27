using System.Collections.Generic;
using System.Threading.Tasks;

namespace Syndiesis.Controls.AnalysisVisualization;

public sealed class CurrentNodeDetailsSection : NodeDetailsSection
{
    public AnalysisTreeListNode NodeDisplayNode => Nodes[0];

    public CurrentNodeDetailsSection()
    {
        HeaderText = "Current Node";
    }

    protected override IReadOnlyList<AnalysisTreeListNode> CreateInitialNodes()
    {
        return CreateNodes(1);
    }

    public override async Task LoadData(NodeDetailsViewData data)
    {
        var section = data.CurrentNode;
        await LoadNodes([
            section.CurrentNode,
        ]);
    }
}
