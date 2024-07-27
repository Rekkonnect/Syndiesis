using System.Collections.Generic;
using System.Threading.Tasks;

namespace Syndiesis.Controls.AnalysisVisualization;

public sealed class SemanticModelNodeDetailsSection : NodeDetailsSection
{
    public SemanticModelNodeDetailsSection()
    {
        HeaderText = "Semantic Model";
    }

    protected override IReadOnlyList<AnalysisTreeListNode> CreateInitialNodes()
    {
        return CreateNodes(7);
    }

    public override async Task LoadData(NodeDetailsViewData data)
    {
        var section = data.SemanticModel;
        await LoadNodes([
            section.SymbolInfo,
            section.DeclaredSymbolInfo,
            section.TypeInfo,
            section.AliasInfo,
            section.PreprocessingSymbolInfo,
            section.Conversion,
            section.Operation,
        ]);
    }
}
