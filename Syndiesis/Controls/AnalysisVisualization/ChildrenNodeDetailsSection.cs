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

    public override async Task LoadData(NodeDetailsViewData data)
    {
        var section = data.Children;
        await LoadNodes([
            section.ChildNodes,
            section.ChildTokens,
            section.ChildNodesAndTokens,
        ]);
    }
}
