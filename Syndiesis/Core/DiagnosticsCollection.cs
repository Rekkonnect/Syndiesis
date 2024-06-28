using Garyon.Extensions;
using Jamarino.IntervalTree;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Syndiesis.Core;

public sealed class DiagnosticsCollection
{
    private readonly LinearIntervalTree<int, Diagnostic> _diagnostics;

    private DiagnosticsCollection(
        LinearIntervalTree<int, Diagnostic> diagnostics)
    {
        _diagnostics = diagnostics;
    }

    public IReadOnlyList<Diagnostic> DiagnosticsForLine(int line)
    {
        return _diagnostics.Query(line)
            .ToReadOnlyListOrExisting();
    }

    public static DiagnosticsCollection CreateForDiagnostics(ImmutableArray<Diagnostic> diagnostics)
    {
        var tree = new LinearIntervalTree<int, Diagnostic>(diagnostics.Length);
        foreach (var diagnostic in diagnostics)
        {
            var span = diagnostic.Location.GetLineSpan();
            tree.Add(
                span.StartLinePosition.Line,
                span.EndLinePosition.Line,
                diagnostic);
        }

        return new(tree);
    }
}
