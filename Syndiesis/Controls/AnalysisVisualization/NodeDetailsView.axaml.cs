using Avalonia.Controls;

namespace Syndiesis.Controls.AnalysisVisualization;

public partial class NodeDetailsView : UserControl
{
    public NodeDetailsView()
    {
        InitializeComponent();
    }

    public void Load(NodeDetailsViewData viewData)
    {
        currentNodeSection.SetNode(viewData.CurrentNode);
        parentSection.SetNode(viewData.ParentNode);
        propertiesSection.SetNodes(viewData.Properties);
    }
}
