using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Syndiesis.Core;

public sealed class SingleTreeCompilationSource
{
    public Compilation Compilation { get; private set; }

    public SyntaxTree? Tree { get; private set; }

    public SingleTreeCompilationSource()
    {
        InitializeCompilation();
    }

    [MemberNotNull(nameof(Compilation))]
    private void InitializeCompilation()
    {
        Compilation = CSharpCompilation.Create("Syndiesis.UserSource")
            .WithReferences(CompilationReferences.CurrentNetVersion);
    }

    [MemberNotNull(nameof(Tree))]
    public void SetSource(string source, CancellationToken cancellationToken)
    {
        var previousTree = Tree;
        SetTree(source, cancellationToken);
        Compilation = AddOrReplaceSyntaxTree(Compilation, previousTree, Tree);
    }

    private static Compilation AddOrReplaceSyntaxTree(
        Compilation compilation, SyntaxTree? previousTree, SyntaxTree newTree)
    {
        if (previousTree is null)
        {
            return compilation.AddSyntaxTrees(newTree);
        }
        else
        {
            return compilation.ReplaceSyntaxTree(previousTree, newTree);
        }
    }

    [MemberNotNull(nameof(Tree))]
    private void SetTree(string source, CancellationToken cancellationToken)
    {
        if (Tree is null)
        {
            Tree = CSharpSyntaxTree.ParseText(source, cancellationToken: cancellationToken);
        }
        else
        {
            var text = SourceText.From(source);
            Tree = Tree.WithChangedText(text);
        }
    }
}
