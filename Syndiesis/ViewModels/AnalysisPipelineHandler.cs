using Syndiesis.Controls;
using Syndiesis.Controls.SyntaxVisualization.Creation;
using Syndiesis.Utilities;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Threading.Tasks;

namespace Syndiesis.ViewModels;

public class AnalysisPipelineHandler
{
    private readonly CancellationTokenFactory _analysisCancellationTokenFactory = new();

    private readonly Delayer _delayer = new();

    private string _pendingSource = string.Empty;
    private volatile bool _finishedAnalysis = false;

    public TimeSpan UserInputDelay { get; set; } = TimeSpan.FromMilliseconds(300);

    public event Action? AnalysisRequested;
    public event Action? AnalysisBegun;
    public event Action<SyntaxTreeListNode>? AnalysisCompleted;

    public void InitiateAnalysis(string source)
    {
        _pendingSource = source;
        if (_delayer.IsWaiting)
        {
            SetRequestedDelay();
            return;
        }

        // only cancel the analysis token if we have to interrupt an analysis
        // in the middle of its execution
        if (!_finishedAnalysis)
        {
            _analysisCancellationTokenFactory.Cancel();
        }
        _ = PerformAnalysis();
    }

    private async Task PerformAnalysis()
    {
        _finishedAnalysis = false;
        var token = _analysisCancellationTokenFactory.CurrentToken;

        AnalysisRequested?.Invoke();

        SetRequestedDelay();
        await _delayer.WaitUnblock(token);
        if (token.IsCancellationRequested)
            return;

        AnalysisBegun?.Invoke();

        var options = new NodeLineCreationOptions();
        var creator = new NodeLineCreator(options);

        var source = _pendingSource;
        var syntaxTree = CSharpSyntaxTree.ParseText(source, cancellationToken: token);
        if (token.IsCancellationRequested)
            return;
        var compilationUnitRoot = syntaxTree.GetCompilationUnitRoot(token);
        if (token.IsCancellationRequested)
            return;

        var nodeRoot = creator.CreateRootNode(compilationUnitRoot);
        if (token.IsCancellationRequested)
            return;

        AnalysisCompleted!(nodeRoot);
        _finishedAnalysis = true;
    }

    private void SetRequestedDelay()
    {
        _delayer.SetFutureUnblock(UserInputDelay);
    }
}
