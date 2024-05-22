using Syndiesis.Core.DisplayAnalysis;

namespace Syndiesis.Core;

public sealed class SymbolAnalysisResult(UIBuilder.AnalysisTreeListNode nodeRoot)
    : AnalysisResult
{
    public UIBuilder.AnalysisTreeListNode NodeRoot { get; set; } = nodeRoot;
}
