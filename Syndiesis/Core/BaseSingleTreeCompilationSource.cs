using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Syndiesis.Core;

public abstract class BaseSingleTreeCompilationSource<TCompilation, TParseOptions>
    : ISingleTreeCompilationSource
    where TCompilation : Compilation
    where TParseOptions : ParseOptions
{
    private DiagnosticsCollection? _diagnostics = null;

    public abstract string LanguageName { get; }

    public TCompilation Compilation { get; private set; }
    Compilation ISingleTreeCompilationSource.Compilation => Compilation;

    public SyntaxTree? Tree { get; private set; }

    public SemanticModel? SemanticModel
    {
        get
        {
            if (Tree is null)
                return null;

            return Compilation.GetSemanticModel(Tree);
        }
    }

    public DiagnosticsCollection Diagnostics => RediscoverDiagnostics();

    public TParseOptions ParseOptions { get; set; }

    public abstract RoslynLanguageVersion LanguageVersion { get; }

    protected BaseSingleTreeCompilationSource()
    {
        InitializeCompilation();
        ParseOptions = CreateDefaultParseOptions();
    }

    [MemberNotNull(nameof(Compilation))]
    private void InitializeCompilation()
    {
        Compilation = (TCompilation)CreateCompilation()
            .WithReferences(CompilationReferences.Runnable)
            ;
    }

    protected abstract TParseOptions CreateDefaultParseOptions();

    public void AdjustLanguageVersion(RoslynLanguageVersion version)
    {
        AdjustLanguageVersionCore(version);
        InitializeCompilation();

        if (Tree is not null)
        {
            var text = Tree.GetText(default);
            Tree = null;
            SetSource(text.ToString(), default);
        }
    }

    protected abstract void AdjustLanguageVersionCore(RoslynLanguageVersion version);

    protected abstract TCompilation CreateCompilation();
    protected abstract SyntaxTree ParseTree(string source, CancellationToken cancellationToken);

    public void AdjustOutputKindCore(OutputKind outputKind)
    {
        Compilation = Compilation.ModifyOptions(options => options.WithOutputKind(outputKind));
    }

    private DiagnosticsCollection RediscoverDiagnostics()
    {
        if (_diagnostics is not null)
            return _diagnostics;

        var diagnostics = Compilation.GetDiagnostics();
        _diagnostics = DiagnosticsCollection.CreateForDiagnostics(diagnostics);
        return _diagnostics;
    }

    public void ReParseCurrent(CancellationToken token)
    {
        var text = Tree!.GetText(token);
        if (token.IsCancellationRequested)
            return;

        SetSource(text.ToString(), token);
    }

    [MemberNotNull(nameof(Tree))]
    public void SetSource(string source, CancellationToken cancellationToken)
    {
        var previousTree = Tree;
        SetTree(source, cancellationToken);
        Compilation = AddOrReplaceSyntaxTree(Compilation, previousTree, Tree);
        _diagnostics = null;
    }

    private static TCompilation AddOrReplaceSyntaxTree(
        TCompilation compilation, SyntaxTree? previousTree, SyntaxTree newTree)
    {
        if (previousTree is null)
        {
            return (TCompilation)compilation.AddSyntaxTrees(newTree);
        }
        else
        {
            return (TCompilation)compilation.ReplaceSyntaxTree(previousTree, newTree);
        }
    }

    [MemberNotNull(nameof(Tree))]
    private void SetTree(string source, CancellationToken cancellationToken)
    {
        if (Tree is null)
        {
            Tree = ParseTree(source, cancellationToken: cancellationToken);
        }
        else
        {
            var text = SourceText.From(source);
            Tree = Tree.WithChangedText(text);
        }
    }
}
