using AvaloniaEdit.Document;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Core;
using System.Collections.Immutable;

[assembly: Parallelizable(ParallelScope.Fixtures)]

namespace Syndiesis.Tests;

public abstract class BaseProjectCodeTests
{
    protected static ProjectSourceProvider SourceProvider
        = ProjectSourceProvider.Get();

    protected static ImmutableArray<FileInfo> FilesToTest
        = SourceProvider.GetFilePaths();

    [Test]
    public async Task TestAllFilesIndependently()
    {
        var sourceTests = new List<Task>();
        foreach (var file in FilesToTest)
        {
            var text = await File.ReadAllTextAsync(file.FullName);
            var testTask = TestSource(text);
            sourceTests.Add(testTask);
        }

        await Task.WhenAll(sourceTests);
    }

    protected abstract Task TestSource(string text);
}

[TestFixtureSource(nameof(AnalysisNodeKindSource))]
public sealed class AnalysisPipelineHandlerTests(AnalysisNodeKind analysisNodeKind)
    : BaseProjectCodeTests
{
    public AnalysisNodeKind AnalysisNodeKind { get; } = analysisNodeKind;

    protected override async Task TestSource(string text)
    {
        var pipeline = CreatePipeline();
        await pipeline.ForceAnalysis(new StringTextSource(text));
    }

    private AnalysisPipelineHandler CreatePipeline()
    {
        var pipeline = new AnalysisPipelineHandler();
        pipeline.AnalysisCompleted += HandleAnalysisCompleted;
        pipeline.AnalysisFailed += HandleAnalysisFailed;
        var hybridCompilation = new HybridSingleTreeCompilationSource();
        var executionFactory = new AnalysisExecutionFactory(hybridCompilation);
        pipeline.AnalysisExecution = executionFactory
            .CreateAnalysisExecution(AnalysisNodeKind);
        return pipeline;
    }

    private void HandleAnalysisFailed(FailedAnalysisResult result)
    {
        Assert.Fail("An analysis failed");
    }

    private void HandleAnalysisCompleted(AnalysisResult result)
    {
    }

    [Test]
    public async Task TestAllFilesWithFlow()
    {
        TestContext.Progress.WriteLine(
            $"Began testing the analysis pipeline on all files, this will take some time.");

        var pipeline = CreatePipeline();

        foreach (var file in FilesToTest)
        {
            var text = await File.ReadAllTextAsync(file.FullName);
            var textSource = new StringTextSource(text);
            await pipeline.ForceAnalysis(textSource);

            TestContext.Progress.WriteLine($"Processed file {file.FullName}");
        }
    }

    private static IEnumerable<AnalysisNodeKind> AnalysisNodeKindSource()
    {
        return
        [
            AnalysisNodeKind.Syntax,
            AnalysisNodeKind.Symbol,
            AnalysisNodeKind.Operation,
            AnalysisNodeKind.Attribute,
        ];
    }

}
