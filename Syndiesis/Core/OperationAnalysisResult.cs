using Syndiesis.Core.DisplayAnalysis;

namespace Syndiesis.Core;

public sealed class OperationAnalysisResult(UIBuilder.AnalysisTreeListNode nodeRoot)
    : AnalysisResult
{
    public UIBuilder.AnalysisTreeListNode NodeRoot { get; set; } = nodeRoot;
}
