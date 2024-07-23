using System.Collections.Generic;

namespace Syndiesis.Controls.AnalysisVisualization;

public sealed class SemanticModelNodeDetailsSection : NodeDetailsSection
{
    public SemanticModelNodeDetailsSection()
    {
        HeaderText = "Semantic Model";
    }

    protected override IReadOnlyList<AnalysisTreeListNode> CreateInitialNodes()
    {
        return CreateNodes(6);
    }

    public override void LoadData(NodeDetailsViewData data)
    {
        var section = data.SemanticModel;
        LoadNodes([
            section.SymbolInfo,
            section.TypeInfo,
            section.AliasInfo,
            section.PreprocessingSymbolInfo,
            section.Conversion,
            section.Operation,
            ]);
    }
}
