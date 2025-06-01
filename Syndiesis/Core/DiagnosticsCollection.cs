using Garyon.Extensions;
using Jamarino.IntervalTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
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

    public IReadOnlyList<Diagnostic> DiagnosticsAtPosition(LinePosition position)
    {
        var diagnostics = DiagnosticsForLine(position.Line);
        if (diagnostics is [])
            return diagnostics;

        var first = diagnostics[0];
        var sourceTree = first.Location.SourceTree;
        if (sourceTree is null)
            return diagnostics;

        return diagnostics
            .Where(d => d.Location.GetLineSpan().Span.Contains(position))
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
