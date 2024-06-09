using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Core.DisplayAnalysis;

namespace Syndiesis.Core;

public sealed class OperationAnalysisResult(UIBuilder.AnalysisTreeListNode nodeRoot)
    : NodeRootAnalysisResult(nodeRoot)
{
    public override AnalysisNodeKind TargetAnalysisNodeKind => AnalysisNodeKind.Operation;
}
