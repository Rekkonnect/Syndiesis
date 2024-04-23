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
        listView.RootNode = node;
        var hideDuration = TimeSpan.FromMilliseconds(500);
        _ = coverable.HideCover(hideDuration);
    }

    private void HandleAnalysisRequested()
    {
        var showDuration = TimeSpan.FromMilliseconds(100);
        var image = TopLevel.GetTopLevel(this)!.FindResource("PenImage") as Image;
        const string requestedText = """
            Awaiting for further user input in the code editor
            """;
        _ = coverable.ShowCover(image, requestedText, showDuration);
    }

    private void HandleAnalysisBegun()
    {
        var showDuration = TimeSpan.FromMilliseconds(100);
        var spinner = new LoadingSpinner();
        const string begunText = """
            Parsing and analyzing the syntax tree,
            we should be ready soon
            """;
        _ = coverable.ShowCover(spinner, begunText, showDuration);
    }
}
