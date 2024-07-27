using System.Collections.Generic;
using System.Threading.Tasks;

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

    public override async Task LoadData(NodeDetailsViewData data)
    {
        var section = data.ParentNode;
        await LoadNodes([
            section.ParentNode
        ]);
    }
}
