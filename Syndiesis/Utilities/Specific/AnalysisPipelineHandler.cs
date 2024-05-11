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

    public IAnalysisExecution AnalysisExecution { get; set; }
        = new SyntaxNodeAnalysisExecution();

    public event Action? AnalysisRequested;
    public event Action? AnalysisBegun;
    public event Action<AnalysisResult>? AnalysisCompleted;
    public event Action<FailedAnalysisResult>? AnalysisFailed;

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
            var result = await AnalysisExecution.Execute(_pendingSource, token);
            AnalysisCompleted!(result);
        }
        catch (Exception e)
        {
            AnalysisFailed?.Invoke(new(e));
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
