using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Syndiesis.Core;

public sealed class DiagnosticIntervalList
{
    private readonly List<Entry> _entries;

    public IReadOnlyList<Entry> Entries => _entries;

    private DiagnosticIntervalList(int capacity)
    {
        _entries = new(capacity);
    }

    public static DiagnosticIntervalList ForDiagnostics(IReadOnlyList<Diagnostic> diagnostics)
    {
        var list = new DiagnosticIntervalList(diagnostics.Count);
        list.AddDiagnostics(diagnostics);
        return list;
    }

    public void AddDiagnostics(IReadOnlyList<Diagnostic> diagnostics)
    {
        var informationDiagnostics = new List<Diagnostic>(diagnostics.Count);
        var warningDiagnostics = new List<Diagnostic>(diagnostics.Count);
        var errorDiagnostics = new List<Diagnostic>(diagnostics.Count);

        foreach (var diagnostic in diagnostics)
        {
            switch (diagnostic.Severity)
            {
                case DiagnosticSeverity.Info:
                    informationDiagnostics.Add(diagnostic);
                    break;
                case DiagnosticSeverity.Warning:
                    warningDiagnostics.Add(diagnostic);
                    break;
                case DiagnosticSeverity.Error:
                    errorDiagnostics.Add(diagnostic);
                    break;
            }
        }

        AddDiagnosticList(informationDiagnostics);
        AddDiagnosticList(warningDiagnostics);
        AddDiagnosticList(errorDiagnostics);
    }

    private void AddDiagnosticList(List<Diagnostic> diagnostics)
    {
        diagnostics.Sort(DiagnosticSorter.Instance);

        foreach (var diagnostic in diagnostics)
        {
            AddDiagnostic(diagnostic);
        }

        MergePass();
    }

    private sealed class DiagnosticSorter : IComparer<Diagnostic>
    {
        public static readonly DiagnosticSorter Instance = new();

        int IComparer<Diagnostic>.Compare(Diagnostic? x, Diagnostic? y)
        {
            return x!.Location.SourceSpan.Start.CompareTo(
                y!.Location.SourceSpan.Start);
        }
    }

    private void MergePass()
    {
        for (int i = 0; i < _entries.Count - 1; i++)
        {
            var current = _entries[i];
            var next = _entries[i + 1];
            Debug.Assert(current.Span.Start <= next.Span.Start, "The entries must be sorted by start.");
            if (current.TryMerge(next, out var merged))
            {
                _entries[i] = merged;
                _entries.RemoveAt(i + 1);
                i--;
            }
        }
    }

    private void AddDiagnostic(Diagnostic diagnostic)
    {
        var entry = new Entry(diagnostic);
        bool inserted = false;

        for (int i = 0; i < _entries.Count; i++)
        {
            var existing = _entries[i];

            if (entry.Span == existing.Span)
            {
                _entries[i] = entry;
                inserted = true;
                continue;
            }

            // replace the covered entry, or outright remove if already inserted
            if (entry.Covers(existing))
            {
                _entries.RemoveAt(i);
                i--;
                if (!inserted)
                {
                    i++;
                    _entries.Insert(i, entry);
                    inserted = true;
                }
                continue;
            }

            // otherwise try reducing the existing entry
            if (existing.TryReduceFor(entry, out var split))
            {
                var splitInsertionIndex = i + 1;
                if (!inserted)
                {
                    _entries.Insert(i + 1, entry);
                    splitInsertionIndex++;
                    inserted = true;
                }
                if (split is not null)
                {
                    _entries.Insert(splitInsertionIndex, split);
                }
            }

            if (!inserted)
            {
                if (entry.Span.End <= existing.Span.Start)
                {
                    _entries.Insert(i, entry);
                    inserted = true;
                }
            }
        }

        if (!inserted)
        {
            _entries.Add(entry);
        }
    }

    public sealed class Entry
    {
        public bool IsEmpty => Span.IsEmpty;

        public DiagnosticSeverity Severity { get; }
        public TextSpan Span { get; private set; }

        public Entry(Diagnostic diagnostic)
        {
            Severity = diagnostic.Severity;
            Span = diagnostic.Location.SourceSpan;
        }

        public Entry(DiagnosticSeverity severity, TextSpan span)
        {
            Severity = severity;
            Span = span;
        }

        public Entry NewWithStart(int start)
        {
            Debug.Assert(start <= Span.End);
            var span = TextSpan.FromBounds(start, Span.End);
            return new(Severity, span);
        }

        public Entry NewWithEnd(int end)
        {
            Debug.Assert(Span.Start <= end);
            var span = TextSpan.FromBounds(Span.Start, end);
            return new(Severity, span);
        }

        public bool TryMerge(Entry next, [NotNullWhen(true)] out Entry? merged)
        {
            if (Severity != next.Severity)
            {
                merged = null;
                return false;
            }

            if (Span.End >= next.Span.Start)
            {
                merged = NewWithEnd(next.Span.End);
                return true;
            }

            merged = null;
            return false;
        }

        public bool OverlapsWith(Entry other)
        {
            return Span.OverlapsWith(other.Span);
        }

        public bool Covers(Entry other)
        {
            return other.Span != Span
                && Span.Contains(other.Span)
                ;
        }

        public void ReduceFor(Entry other, out Entry? split)
        {
            // We don't want to reduce the span if the severity is not higher
            Debug.Assert(other.Severity >= Severity);
            Debug.Assert(OverlapsWith(other));

            var span = Span;
            var otherSpan = other.Span;

            bool thisSuperset = Superset(span, otherSpan);
            // this span is a proper superset of other span
            if (thisSuperset)
            {
                var newThisSpan = TextSpan.FromBounds(span.Start, otherSpan.Start);
                Span = newThisSpan;
                split = NewWithStart(otherSpan.End);
                return;
            }

            Debug.Assert(
                !Superset(otherSpan, span),
                "This method should have not been invoked if the other span completely covers this one.");

            // the spans overlap but we don't have a proper superset, so we only trim the one side
            split = null;
            if (span.Start < otherSpan.Start)
            {
                Debug.Assert(
                    span.End <= otherSpan.End,
                    "The two spans must be overlapping on the one side");
                Span = TextSpan.FromBounds(span.Start, otherSpan.Start);
            }
            else
            {
                Debug.Assert(
                    span.Start >= otherSpan.Start,
                    "The two spans must be overlapping on the one side");
                Span = TextSpan.FromBounds(otherSpan.End, span.End);
            }
        }

        private static bool Superset(TextSpan outer, TextSpan inner)
        {
            return outer.Start < inner.Start
                && outer.End > inner.End
                ;
        }

        public bool TryReduceFor(Entry other, out Entry? split)
        {
            if (!OverlapsWith(other))
            {
                split = null;
                return false;
            }

            ReduceFor(other, out split);
            return true;
        }

        public override string ToString()
        {
            return $"[{Span.Start}, {Span.End}) - {Severity}";
        }
    }
}
