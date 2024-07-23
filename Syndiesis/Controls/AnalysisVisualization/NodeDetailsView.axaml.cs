using Avalonia.Controls;
using System.Collections.Generic;

namespace Syndiesis.Controls.AnalysisVisualization;

public partial class NodeDetailsView : UserControl, IAnalysisNodeHoverManager
{
    public NodeDetailsView()
    {
        InitializeComponent();
        RegisterSections();
    }

    private void RegisterSections()
    {
        var sections = DetailsSections();
        foreach (var section in sections)
        {
            section.HoverManager = this;
        }
    }

    public void Load(NodeDetailsViewData viewData)
    {
        var sections = DetailsSections();
        foreach (var section in sections)
        {
            section.LoadData(viewData);
        }
    }

    private IReadOnlyList<NodeDetailsSection> DetailsSections()
    {
        return
        [
            currentNodeSection,
            parentSection,
            childrenSection,
            semanticModelSection,
        ];
    }

    #region Node hovers
    private bool _allowedHover = true;
    private AnalysisTreeListNode? _hoveredNode;

    public bool RequestHover(AnalysisTreeListNode node)
    {
        if (!_allowedHover)
            return false;

        var parent = node.ParentNode;
        if (parent is null)
            return true;

        return parent.NodeLine.IsExpanded;
    }

    public bool IsHovered(AnalysisTreeListNode node)
    {
        return _hoveredNode == node;
    }

    public void ClearHover()
    {
        _hoveredNode?.UpdateHovering(false);
        _hoveredNode = null;
    }

    public void RemoveHover(AnalysisTreeListNode node)
    {
        if (node != _hoveredNode)
            return;

        _hoveredNode = null;
        node.UpdateHovering(false);
    }

    public void OverrideHover(AnalysisTreeListNode node)
    {
        if (_hoveredNode == node)
            return;

        var previousHover = _hoveredNode;
        previousHover?.UpdateHovering(false);
        _hoveredNode = node;
        node.UpdateHovering(true);
    }
    #endregion
}
