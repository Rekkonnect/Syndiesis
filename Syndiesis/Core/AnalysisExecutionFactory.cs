using Syndiesis.Controls.AnalysisVisualization;
using System;

namespace Syndiesis.Core;

public sealed class AnalysisExecutionFactory(
    HybridSingleTreeCompilationSource compilationSource)
{
    private readonly HybridSingleTreeCompilationSource _compilationSource = compilationSource;

    public BaseAnalysisExecution CreateAnalysisExecution(AnalysisNodeKind kind)
    {
        switch (kind)
        {
            case AnalysisNodeKind.Syntax:
                return new SyntaxNodeAnalysisExecution(_compilationSource);
            case AnalysisNodeKind.Operation:
                return new OperationAnalysisExecution(_compilationSource);
            case AnalysisNodeKind.Symbol:
                return new SymbolAnalysisExecution(_compilationSource);
            default:
                throw ThrowUnsupportedAnalysisKind();
        }
    }

    private static Exception ThrowUnsupportedAnalysisKind()
    {
        throw new NotSupportedException("The given analysis kind is unsupported.");
    }
}
