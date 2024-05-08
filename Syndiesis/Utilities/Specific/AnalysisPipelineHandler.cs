using Microsoft.CodeAnalysis.CSharp;
using Syndiesis.Controls;
using Syndiesis.Controls.SyntaxVisualization.Creation;
using System;
using System.Threading.Tasks;

namespace Syndiesis.Utilities.Specific;

public class AnalysisPipelineHandler
{
    private readonly CancellationTokenFactory _analysisCancellationTokenFactory = new();

    private readonly Delayer _delayer = new();

    private string _pendingSource = string.Empty;
    private volatile bool _finishedAnalysis = false;

    private volatile int _ignoredInputDelayTimes = 0;

    public TimeSpan UserInputDelay { get; set; } = AppSettings.Instance.UserInputDelay;

    public NodeLineCreationOptions CreationOptions { get; set; } = new();

    public event Action? AnalysisRequested;
    public event Action? AnalysisBegun;
    public event Action<SyntaxTreeListNode>? AnalysisCompleted;
    public event Action<Exception>? AnalysisFailed;

    public void IgnoreInputDelayOnce()
    {
        _ignoredInputDelayTimes++;
    }

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

        try
        {
            var creator = new NodeLineCreator(CreationOptions);

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
        }
        catch (Exception e)
        {
            AnalysisFailed?.Invoke(e);
        }
        _finishedAnalysis = true;
    }

    private void SetRequestedDelay()
    {
        if (_ignoredInputDelayTimes > 0)
        {
            _ignoredInputDelayTimes--;
            return;
        }
        
        _delayer.SetFutureUnblock(UserInputDelay);
    }
}
