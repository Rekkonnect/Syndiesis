using Avalonia.Controls;
using CSharpSyntaxEditor.ViewModels;
using System;

namespace CSharpSyntaxEditor.Controls.SyntaxVisualization;

public partial class CoverableSyntaxTreeListView : UserControl
{
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
    }

    private void HandleAnalysisCompleted(SyntaxTreeListNode node)
    {
        var image = App.CurrentResourceManager.SuccessImage?.CopyOfSource();
        coverable.UpdateCoverContent(image, "Analysis complete");

        listView.RootNode = node;
        var hideDuration = TimeSpan.FromMilliseconds(500);
        _ = coverable.HideCover(hideDuration);
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
