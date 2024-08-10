using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Syndiesis.Core;

namespace Syndiesis.Tests;

[Parallelizable(ParallelScope.Children)]
public sealed class NodeViewDetailsHandlerTests
    : BaseProjectCodeTests
{
    protected override async Task TestSource(string text)
    {
        var hybridCompilation = new HybridSingleTreeCompilationSource();
        hybridCompilation.SetSource(text, default);
        await TestEntireHybridCompilationTree(hybridCompilation);
    }

    private static async Task TestEntireHybridCompilationTree(
        HybridSingleTreeCompilationSource hybridCompilation)
    {
        var tree = hybridCompilation.CurrentSource.Tree;
        Assert.That(tree, Is.Not.Null);
        var root = await tree.GetRootAsync();
        Assert.That(root, Is.Not.Null);

        var nodes = root.DescendantNodesAndSelf(descendIntoTrivia: true)
            .ToList();
        await Parallel.ForEachAsync(
            nodes,
            TestNodeLocal);

        async ValueTask TestNodeLocal(SyntaxNode node, CancellationToken cancellationToken)
        {
            await TestNode(hybridCompilation, node);
        }
    }

    private static async Task TestNode(
        HybridSingleTreeCompilationSource hybridCompilation,
        SyntaxNode node)
    {
        var span = node.Span;
        var result = await TestExecutingResult(hybridCompilation, span);
        var rootNode = result.Root!.Node;
        Assert.That(rootNode, Is.Not.Null);

        // For nodes with zero length, this is the equivalent of hovering the caret
        // over the node that will be selected, and thus we care about containing the
        // intended node's span
        if (span.Length is 0)
        {
            Assert.That(rootNode.Span.Contains(span), Is.True);
        }
        else
        {
            Assert.That(rootNode?.FullSpan, Is.EqualTo(node.FullSpan));
        }
    }

    private static async Task<NodeViewAnalysisExecution> TestExecutingResult(
        HybridSingleTreeCompilationSource hybridCompilation,
        TextSpan span)
    {
        var execution = NodeViewAnalysisHelpers
            .GetNodeViewAnalysisExecutionForSpan(hybridCompilation, span);
        Assert.That(execution, Is.Not.Null);

        var result = execution.ExecuteCore(default);
        Assert.That(result, Is.Not.Null);

        bool allSuccessful = await result.AwaitAllLoaded();
        Assert.That(allSuccessful, Is.True);
        return execution;
    }

    [Test]
    public async Task TestAllFilesWithFlow()
    {
        TestContext.Progress.WriteLine(
            "Began testing the node view data analysis on all files sequentially, this will take some more time.");

        var hybridCompilation = new HybridSingleTreeCompilationSource();

        foreach (var file in FilesToTest)
        {
            var text = await File.ReadAllTextAsync(file.FullName);
            hybridCompilation.SetSource(text, default);
            await TestEntireHybridCompilationTree(hybridCompilation);

            TestContext.Progress.WriteLine($"Processed file {file.FullName}");
        }
    }
}
