using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Core.DisplayAnalysis;

namespace Syndiesis.Core;

public abstract class NodeRootAnalysisResult(UIBuilder.AnalysisTreeListNode nodeRoot)
    : AnalysisResult
{
    public UIBuilder.AnalysisTreeListNode NodeRoot { get; set; } = nodeRoot;

    public abstract AnalysisNodeKind TargetAnalysisNodeKind { get; }
}
