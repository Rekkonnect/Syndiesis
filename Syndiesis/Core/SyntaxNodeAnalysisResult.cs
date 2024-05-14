using Syndiesis.Controls;

namespace Syndiesis.Core;

public sealed class SyntaxNodeAnalysisResult(SyntaxTreeListNode nodeRoot)
    : AnalysisResult
{
    public SyntaxTreeListNode NodeRoot { get; set; } = nodeRoot;
}
