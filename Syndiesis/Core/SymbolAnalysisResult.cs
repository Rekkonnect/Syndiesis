using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Core.DisplayAnalysis;

namespace Syndiesis.Core;

public sealed class SymbolAnalysisResult(UIBuilder.AnalysisTreeListNode nodeRoot)
    : NodeRootAnalysisResult(nodeRoot)
{
    public override AnalysisNodeKind TargetAnalysisNodeKind => AnalysisNodeKind.Symbol;
}