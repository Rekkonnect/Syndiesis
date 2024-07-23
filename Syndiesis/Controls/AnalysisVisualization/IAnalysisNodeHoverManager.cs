namespace Syndiesis.Controls.AnalysisVisualization;

internal interface IAnalysisNodeHoverManager
{
    public bool RequestHover(AnalysisTreeListNode node);

    public bool IsHovered(AnalysisTreeListNode node);

    public void ClearHover();

    public void RemoveHover(AnalysisTreeListNode node);

    public void OverrideHover(AnalysisTreeListNode node);
}
