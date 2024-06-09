using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Core.DisplayAnalysis;

namespace Syndiesis.Core;

public sealed class AttributeAnalysisResult(UIBuilder.AnalysisTreeListNode nodeRoot)
    : NodeRootAnalysisResult(nodeRoot)
{
    public override AnalysisNodeKind TargetAnalysisNodeKind => AnalysisNodeKind.Attribute;
}
