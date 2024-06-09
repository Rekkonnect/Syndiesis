using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Core.DisplayAnalysis;

namespace Syndiesis.Core;

public sealed class SyntaxNodeAnalysisResult(UIBuilder.AnalysisTreeListNode nodeRoot)
    : NodeRootAnalysisResult(nodeRoot)
{
    public override AnalysisNodeKind TargetAnalysisNodeKind => AnalysisNodeKind.Syntax;
}
