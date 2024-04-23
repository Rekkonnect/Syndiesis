using CSharpSyntaxEditor.Controls;
using CSharpSyntaxEditor.Controls.SyntaxVisualization.Creation;
using CSharpSyntaxEditor.Utilities;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Threading.Tasks;

namespace CSharpSyntaxEditor.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public readonly MultilineStringEditor Editor = new();
}

public class AnalysisPipelineHandler
{
    private readonly CancellationTokenFactory _analysisCancellationTokenFactory = new();

    public TimeSpan UserInputDelay { get; set; } = TimeSpan.FromMilliseconds(300);

    public event Action? AnalysisRequested;
    public event Action? AnalysisBegun;
    public event Action<SyntaxTreeListNode>? AnalysisCompleted;

    public void InitiateAnalysis(string source)
    {
        _analysisCancellationTokenFactory.Cancel();
        _ = PerformAnalysis(source);
    }

    private async Task PerformAnalysis(string source)
    {
        var token = _analysisCancellationTokenFactory.CurrentToken;

        AnalysisRequested?.Invoke();

        await Task.Delay(UserInputDelay, token);
        token.ThrowIfCancellationRequested();

        AnalysisBegun?.Invoke();

        var options = new NodeLineCreationOptions();
        var creator = new NodeLineCreator(options);

        var syntaxTree = CSharpSyntaxTree.ParseText(source, cancellationToken: token);
        token.ThrowIfCancellationRequested();
        var compilationUnitRoot = syntaxTree.GetCompilationUnitRoot(token);
        token.ThrowIfCancellationRequested();

        var nodeRoot = creator.CreateRootNode(compilationUnitRoot);
        token.ThrowIfCancellationRequested();

        AnalysisCompleted!(nodeRoot);
    }
}
