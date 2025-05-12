using AvaloniaEdit.Document;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Core;

namespace Syndiesis.Tests;

public sealed class AnalysisPipelineHandlerTests
{
    [Test]
    [MethodDataSource(nameof(PipelineTestArgumentsSource))]
    public async Task TestFilePipelineExecution(PipelineTestArguments arguments)
    {
        var (analysisNodeKind, file) = arguments;
        var pipeline = CreatePipeline(analysisNodeKind);

        var fileContent = CacheableFileContentContainer<CacheableFileText>.Shared.Get(file);
        var text = await fileContent.GetTextAsync();
        var textSource = new StringTextSource(text);
        await pipeline.ForceAnalysis(textSource);
    }

    private void HandleAnalysisFailed(FailedAnalysisResult result)
    {
        throw result.Exception
            ?? new Exception("An analysis failed without an exception");
    }

    private void HandleAnalysisCompleted(AnalysisResult result)
    {
    }

    private AnalysisPipelineHandler CreatePipeline(AnalysisNodeKind analysisNodeKind)
    {
        var pipeline = new AnalysisPipelineHandler();
        pipeline.AnalysisCompleted += HandleAnalysisCompleted;
        pipeline.AnalysisFailed += HandleAnalysisFailed;
        var hybridCompilation = new HybridSingleTreeCompilationSource();
        var executionFactory = new AnalysisExecutionFactory(hybridCompilation);
        pipeline.AnalysisExecution = executionFactory
            .CreateAnalysisExecution(analysisNodeKind);
        return pipeline;
    }

    public static IEnumerable<PipelineTestArguments> PipelineTestArgumentsSource()
    {
        foreach (var nodeKind in AnalysisNodeKindSource())
        {
            foreach (var file in TestSources.FilesToTest)
            {
                yield return new(nodeKind, file);
            }
        }
    }

    public static IReadOnlyList<AnalysisNodeKind> AnalysisNodeKindSource()
    {
        return
        [
            AnalysisNodeKind.Syntax,
            AnalysisNodeKind.Symbol,
            AnalysisNodeKind.Operation,
            AnalysisNodeKind.Attribute,
        ];
    }

    public sealed record PipelineTestArguments(
        AnalysisNodeKind NodeKind,
        FileInfo File);
}
