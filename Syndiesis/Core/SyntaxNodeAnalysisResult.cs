using Syndiesis.Controls.AnalysisVisualization;

namespace Syndiesis.Core;

public sealed class SyntaxNodeAnalysisResult(AnalysisTreeListNode nodeRoot)
    : AnalysisResult
{
    public AnalysisTreeListNode NodeRoot { get; set; } = nodeRoot;
}
