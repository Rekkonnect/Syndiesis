using Avalonia.Controls;
using Avalonia.Threading;
using Syndiesis.Core;
using System;

namespace Syndiesis.Controls.AnalysisVisualization;

public partial class CoverableAnalysisView : UserControl
{
    public AnalysisTreeListView ListView = new();
    public NodeDetailsView NodeDetailsView = new();

    public CoverableAnalysisView()
    {
        InitializeComponent();
        InitializeCover();
    }

    private void InitializeCover()
    {
        var spinner = new LoadingSpinner();
        coverable.ShowCover(spinner, "Initializing application", TimeSpan.Zero);

        coverable.ContainedContent = ListView;
    }

    public void SetContent(AnalysisViewKind viewKind)
    {
        coverable.ContainedContent = viewKind switch
        {
            AnalysisViewKind.Tree => ListView,
            AnalysisViewKind.Details => NodeDetailsView,
            _ => throw new ArgumentOutOfRangeException(nameof(viewKind)),
        };
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
        void UIUpdate()
        {
            var image = App.CurrentResourceManager.FailureImage?.CopyOfSource();
            coverable.UpdateCoverContent(
                image,
                "Analysis failed",
                UserInteractionCover.Styling.BadTextBrush);
        }

        Dispatcher.UIThread.Invoke(UIUpdate);
    }

    private void HandleAnalysisCompleted(AnalysisResult analysisResult)
    {
        void UIUpdate()
        {
            var image = App.CurrentResourceManager.SuccessImage?.CopyOfSource();
            coverable.UpdateCoverContent(image, "Analysis complete");

            var hideDuration = TimeSpan.FromMilliseconds(500);
            coverable.HideCover(hideDuration);
        }

        Dispatcher.UIThread.Invoke(UIUpdate);
    }

    private void HandleAnalysisRequested()
    {
        void UIUpdate()
        {
            var showDuration = TimeSpan.FromMilliseconds(100);
            var image = App.CurrentResourceManager.PenImage?.CopyOfSource();
            const string requestedText = """
                Awaiting for further user input in the code editor
                """;
            coverable.ShowCover(image, requestedText, showDuration);
        }

        Dispatcher.UIThread.Invoke(UIUpdate);
    }

    private void HandleAnalysisBegun()
    {
        void UIUpdate()
        {
            var spinner = new LoadingSpinner();
            const string begunText = """
                Parsing and analyzing the given source,
                we should be ready soon
                """;
            coverable.UpdateCoverContent(spinner, begunText);
        }

        Dispatcher.UIThread.Invoke(UIUpdate);
    }
}
