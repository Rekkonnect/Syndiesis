using System;

namespace Syndiesis.Controls.AnalysisVisualization;

internal interface IAnalysisNodeHoverManager
{
    public event Action<AnalysisTreeListNode?>? HoveredNode;

    public bool RequestHover(AnalysisTreeListNode node);

    public bool IsHovered(AnalysisTreeListNode node);

    public void ClearHover();

    public void RemoveHover(AnalysisTreeListNode node);

    public void OverrideHover(AnalysisTreeListNode node);
}
