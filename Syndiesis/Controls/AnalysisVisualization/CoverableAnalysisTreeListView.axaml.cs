using Avalonia.Controls;
using Avalonia.Threading;
using Syndiesis.Core;
using System;

namespace Syndiesis.Controls.AnalysisVisualization;

public partial class CoverableAnalysisTreeListView : UserControl
{
    public event Action? NewRootNodeLoaded;

    public CoverableAnalysisTreeListView()
    {
        InitializeComponent();
        InitializeCover();
    }

    private void InitializeCover()
    {
        var spinner = new LoadingSpinner();
        coverable.ShowCover(spinner, "Initializing application", TimeSpan.Zero);
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

            switch (analysisResult)
            {
                case SyntaxNodeAnalysisResult syntaxNodeAnalysisResult:
                    listView.RootNode = syntaxNodeAnalysisResult.NodeRoot!;
                    listView.TargetAnalysisNodeKind = AnalysisNodeKind.Syntax;
                    break;

                case OperationAnalysisResult operationAnalysisResult:
                    listView.RootNode = operationAnalysisResult.NodeRoot!;
                    listView.TargetAnalysisNodeKind = AnalysisNodeKind.Operation;
                    break;
            }

            var hideDuration = TimeSpan.FromMilliseconds(500);
            coverable.HideCover(hideDuration);
            NewRootNodeLoaded?.Invoke();
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
                Parsing and analyzing the syntax tree,
                we should be ready soon
                """;
            coverable.UpdateCoverContent(spinner, begunText);
        }

        Dispatcher.UIThread.Invoke(UIUpdate);
    }
}
