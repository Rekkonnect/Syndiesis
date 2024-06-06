using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
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

    public string CurrentLanguageName => CurrentSource.LanguageName;

    public ISingleTreeCompilationSource CurrentSource { get; private set; }

    public CSharpSingleTreeCompilationSource CSharpSource => _csSource;
    public VisualBasicSingleTreeCompilationSource VisualBasicSource => _vbSource;

    public HybridSingleTreeCompilationSource()
    {
        CurrentSource = _csSource;
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
        var language = GetLanguage(source, cancellationToken);
        if (cancellationToken.IsCancellationRequested)
            return;

        CurrentSource = SourceForLanguage(language);
        CurrentSource.SetSource(source, cancellationToken);
    }

    private static string GetLanguage(string source, CancellationToken cancellationToken)
    {
        var csTree = CSharpSyntaxTree.ParseText(source, cancellationToken: cancellationToken);
        var vbTree = VisualBasicSyntaxTree.ParseText(source, cancellationToken: cancellationToken);

        if (cancellationToken.IsCancellationRequested)
            return LanguageNames.CSharp;

        int csSkipped = SkippedTokens(
            csTree,
            (int)CSharpSyntaxKind.SkippedTokensTrivia,
            cancellationToken);
        int vbSkipped = SkippedTokens(
            vbTree,
            (int)VisualBasicSyntaxKind.SkippedTokensTrivia,
            cancellationToken);

        if (vbSkipped < csSkipped)
            return LanguageNames.VisualBasic;

        return LanguageNames.CSharp;
    }

    private static int SkippedTokens(
        SyntaxTree tree,
        int skippedTokensKind,
        CancellationToken cancellationToken)
    {
        var csRoot = tree.GetRoot(cancellationToken);

        var trivia = csRoot.DescendantTrivia(descendIntoTrivia: true);
        int count = trivia.Count(s => s.RawKind == skippedTokensKind);
        return count;
    }
}
