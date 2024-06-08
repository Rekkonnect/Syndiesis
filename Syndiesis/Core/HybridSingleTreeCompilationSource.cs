using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using Serilog;
using Syndiesis.Utilities;
using System;
using System.Linq;
using System.Threading;

using CSharpSyntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using VisualBasicSyntaxKind = Microsoft.CodeAnalysis.VisualBasic.SyntaxKind;

namespace Syndiesis.Core;

public sealed class HybridSingleTreeCompilationSource
{
    private readonly CSharpSingleTreeCompilationSource _csSource = new();
    private readonly VisualBasicSingleTreeCompilationSource _vbSource = new();
    private ISingleTreeCompilationSource _currentSource;

    public string CurrentLanguageName => CurrentSource.LanguageName;

    public ISingleTreeCompilationSource CurrentSource
    {
        get => _currentSource;
        private set
        {
            _currentSource = value;
            CompilationSourceChanged?.Invoke();
        }
    }

    public CSharpSingleTreeCompilationSource CSharpSource => _csSource;
    public VisualBasicSingleTreeCompilationSource VisualBasicSource => _vbSource;

    public event Action? CompilationSourceChanged;

    public HybridSingleTreeCompilationSource()
    {
        _currentSource = _csSource;
    }

    public ISingleTreeCompilationSource SourceForLanguage(string languageName)
    {
        return languageName switch
        {
            LanguageNames.CSharp => _csSource,
            LanguageNames.VisualBasic => _vbSource,
            _ => throw new ArgumentException("Invalid language name", nameof(languageName))
        };
    }

    public void SetLanguageVersion(RoslynLanguageVersion version)
    {
        var source = SourceForLanguage(version.LanguageName);
        source.AdjustLanguageVersion(version);
    }

    public void SetSource(string source, CancellationToken cancellationToken)
    {
        if (AppSettings.Instance.AutomaticallyDetectLanguage)
        {
            SetSourceAutomaticDetection(source, cancellationToken);
        }
        else
        {
            SetSourceSimple(source, cancellationToken);
        }
    }

    private void SetSourceSimple(string source, CancellationToken cancellationToken)
    {
        var profiling = new SimpleProfiling();
        using (profiling.BeginProcess())
        {
            CurrentSource.SetSource(source, cancellationToken);
        }
        Log.Information($"Set the source within {profiling.SnapshotResults!.Time.TotalMilliseconds}ms");
    }

    private void SetSourceAutomaticDetection(string source, CancellationToken cancellationToken)
    {
        var profiling = new SimpleProfiling();
        string language;
        using (profiling.BeginProcess())
        {
            language = GetLanguage(source, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                return;
        }
        Log.Information($"Discovered the language within {profiling.SnapshotResults!.Time.TotalMilliseconds}ms");

        using (profiling.BeginProcess())
        {
            CurrentSource = SourceForLanguage(language);
            CurrentSource.SetSource(source, cancellationToken);
        }
        Log.Information($"Set the source within {profiling.SnapshotResults!.Time.TotalMilliseconds}ms");
    }

    private static string GetLanguage(string source, CancellationToken cancellationToken)
    {
        var csTree = CSharpSyntaxTree.ParseText(source, cancellationToken: cancellationToken);
        var vbTree = VisualBasicSyntaxTree.ParseText(source, cancellationToken: cancellationToken);

        if (cancellationToken.IsCancellationRequested)
            return LanguageNames.CSharp;

        var csPenalty = InvalidCodePenalty(
            csTree,
            (int)CSharpSyntaxKind.SkippedTokensTrivia,
            cancellationToken);

        var vbPenalty = InvalidCodePenalty(
            vbTree,
            (int)VisualBasicSyntaxKind.SkippedTokensTrivia,
            cancellationToken);

        if (vbPenalty < csPenalty)
            return LanguageNames.VisualBasic;

        return LanguageNames.CSharp;
    }

    private static double InvalidCodePenalty(
        SyntaxTree tree,
        int skippedTokensKind,
        CancellationToken cancellationToken)
    {
        int skipped = SkippedTokens(
            tree,
            skippedTokensKind,
            cancellationToken);

        int missing = MissingTokens(
            tree,
            cancellationToken);

        return skipped * 1.2 + missing * 1.1;
    }

    private static int SkippedTokens(
        SyntaxTree tree,
        int skippedTokensKind,
        CancellationToken cancellationToken)
    {
        var root = tree.GetRoot(cancellationToken);

        var trivia = root.DescendantTrivia(descendIntoTrivia: true);
        int count = trivia.Count(s => s.RawKind == skippedTokensKind);
        return count;
    }

    private static int MissingTokens(
        SyntaxTree tree,
        CancellationToken cancellationToken)
    {
        var root = tree.GetRoot(cancellationToken);

        var tokens = root.DescendantTokens(descendIntoTrivia: true);
        int count = tokens.Count(s => s.IsMissing);
        return count;
    }
}
