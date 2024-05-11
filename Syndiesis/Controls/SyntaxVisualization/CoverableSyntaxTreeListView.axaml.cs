using Avalonia.Controls;
using Syndiesis.Utilities.Specific;
using System;

namespace Syndiesis.Controls.SyntaxVisualization;

public partial class CoverableSyntaxTreeListView : UserControl
{
    public event Action? NewRootNodeLoaded;

    public CoverableSyntaxTreeListView()
    {
        InitializeComponent();
        InitializeCover();
    }

    private void InitializeCover()
    {
        var spinner = new LoadingSpinner();
        _ = coverable.ShowCover(spinner, "Initializing application", TimeSpan.Zero);
    }

    public void RegisterAnalysisPipelineHandler(
        AnalysisPipelineHandler analysisPipelineHandler)
    {
        analysisPipelineHandler.AnalysisBegun += HandleAnalysisBegun;
        analysisPipelineHandler.AnalysisRequested += HandleAnalysisRequested;
        analysisPipelineHandler.AnalysisCompleted += HandleAnalysisCompleted;
        analysisPipelineHandler.AnalysisFailed += HandleAnalysisFailed;
    }

    private void HandleAnalysisFailed(FailedAnalysisResult failedResult)
    {
        var image = App.CurrentResourceManager.FailureImage?.CopyOfSource();
        coverable.UpdateCoverContent(
            image,
            "Analysis failed",
            UserInteractionCover.Styling.BadTextBrush);
    }

    private void HandleAnalysisCompleted(AnalysisResult analysisResult)
    {
        var image = App.CurrentResourceManager.SuccessImage?.CopyOfSource();
        coverable.UpdateCoverContent(image, "Analysis complete");

        switch (analysisResult)
        {
            case SyntaxNodeAnalysisResult syntaxNodeAnalysisResult:
                listView.RootNode = syntaxNodeAnalysisResult.NodeRoot!;
                break;
        }

        var hideDuration = TimeSpan.FromMilliseconds(500);
        _ = coverable.HideCover(hideDuration);
        NewRootNodeLoaded?.Invoke();
    }

    private void HandleAnalysisRequested()
    {
        var showDuration = TimeSpan.FromMilliseconds(100);
        var image = App.CurrentResourceManager.PenImage?.CopyOfSource();
        const string requestedText = """
            Awaiting for further user input in the code editor
            """;
        _ = coverable.ShowCover(image, requestedText, showDuration);
    }

    private void HandleAnalysisBegun()
    {
        var spinner = new LoadingSpinner();
        const string begunText = """
            Parsing and analyzing the syntax tree,
            we should be ready soon
            """;
        coverable.UpdateCoverContent(spinner, begunText);
    }
}
