using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Syndiesis.Core;
using Syndiesis.Utilities;
using TUnit.Core.Logging;

namespace Syndiesis.Tests;

public sealed class NodeViewDetailsHandlerTests
{
    [Test]
    [MethodDataSource(nameof(FilesToTestSource))]
    public async Task TestAllFilesWithFlow(FileInfo file, CancellationToken cancellationToken)
    {
        var fileContent = CacheableFileContentContainer<CacheableFileText>.Shared.Get(file);
        var text = await fileContent.GetTextAsync(cancellationToken);
        var hybridCompilation = new HybridSingleTreeCompilationSource();
        hybridCompilation.SetSource(text, cancellationToken);
        await TestEntireHybridCompilationTree(hybridCompilation, cancellationToken);
    }

    private static async Task TestEntireHybridCompilationTree(
        HybridSingleTreeCompilationSource hybridCompilation,
        CancellationToken cancellationToken)
    {
        var profiling = new SimpleProfiling();
        int nodeCount = 0;
        int length = 0;
        using (var _ = profiling.BeginProcess())
        {
            var tree = hybridCompilation.CurrentSource.Tree;
            await Assert.That(tree).IsNotNull();
            var root = await tree!.GetRootAsync(cancellationToken);
            await Assert.That(root).IsNotNull();
            length = root.FullSpan.Length;

            var nodes = root.DescendantNodesAndSelf(descendIntoTrivia: true)
                .ToList();
            nodeCount = nodes.Count;
            await Parallel.ForEachAsync(
                nodes,
                cancellationToken,
                TestNodeLocal);
        }

        var seconds = profiling.SnapshotResults!.Time.TotalSeconds;
        TestContext.Current!.GetDefaultLogger().LogInformation($"""
            Finished testing all {nodeCount} nodes from {length} characters in {seconds:N3}s
            """);

        async ValueTask TestNodeLocal(SyntaxNode node, CancellationToken cancellationToken)
        {
            await TestNode(hybridCompilation, node, cancellationToken);
        }
    }

    private static async Task TestNode(
        HybridSingleTreeCompilationSource hybridCompilation,
        SyntaxNode node,
        CancellationToken cancellationToken)
    {
        var span = node.Span;
        var result = await TestExecutingResult(
            hybridCompilation, span, cancellationToken);
        var rootNode = result.Root!.Node;
        await Assert.That(rootNode).IsNotNull();

        // For nodes with zero length, this is the equivalent of hovering the caret
        // over the node that will be selected, and thus we care about containing the
        // intended node's span
        if (span.Length is 0)
        {
            var containedSpan = rootNode!.Span.Contains(span)
                || rootNode.FullSpan.Contains(span);
            await Assert.That(containedSpan).IsTrue();
        }
        else
        {
            await Assert.That(rootNode?.FullSpan).IsEqualTo(node.FullSpan);
        }
    }

    private static async Task<NodeViewAnalysisExecution> TestExecutingResult(
        HybridSingleTreeCompilationSource hybridCompilation,
        TextSpan span,
        CancellationToken cancellationToken)
    {
        var execution = NodeViewAnalysisHelpers
            .GetNodeViewAnalysisExecutionForSpan(hybridCompilation, span);
        await Assert.That(execution).IsNotNull();

        var result = execution!.ExecuteCore(cancellationToken);
        await Assert.That(result).IsNotNull();

        bool allSuccessful = await result!.AwaitAllLoaded(TimeSpan.FromMilliseconds(1));
        await Assert.That(allSuccessful).IsTrue();
        return execution;
    }

    public static IReadOnlyList<FileInfo> FilesToTestSource()
    {
        return TestSources.FilesToTest;
    }
}
