using Syndiesis.Controls;

namespace Syndiesis.Utilities.Specific;

public sealed class SyntaxNodeAnalysisResult(SyntaxTreeListNode nodeRoot)
    : AnalysisResult
{
    public SyntaxTreeListNode NodeRoot { get; set; } = nodeRoot;
}
