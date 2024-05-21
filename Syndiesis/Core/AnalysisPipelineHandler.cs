using Serilog;
using Syndiesis.Utilities;
using System;
using System.Threading.Tasks;

namespace Syndiesis.Core;

public class AnalysisPipelineHandler
{
    private readonly CancellationTokenFactory _analysisCancellationTokenFactory = new();

    private readonly Delayer _delayer = new();

    private string _pendingSource = string.Empty;
    private volatile bool _finishedAnalysis = false;

    private volatile int _ignoredInputDelayTimes = 0;

    public TimeSpan UserInputDelay { get; set; } = AppSettings.Instance.UserInputDelay;

    private SingleTreeCompilationSource _singleTreeCompilationSource = new();

    public IAnalysisExecution AnalysisExecution { get; set; }
#if DEBUG
#else
        = new SyntaxNodeAnalysisExecution();
#endif

    public event Action? AnalysisRequested;
    public event Action? AnalysisBegun;
    public event Action<AnalysisResult>? AnalysisCompleted;
    public event Action<FailedAnalysisResult>? AnalysisFailed;

    public AnalysisPipelineHandler()
    {
#if DEBUG
        AnalysisExecution = new OperationAnalysisExecution(_singleTreeCompilationSource);
#endif
    }

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
        _ = PerformAnalysis()
            .ConfigureAwait(false);
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
            Log.Information($"Began analysis using {AnalysisExecution.GetType()}");
            var profiling = new SimpleProfiling();
            using (profiling.BeginProcess())
            {
                var result = await AnalysisExecution.Execute(_pendingSource, token);
                AnalysisCompleted!(result);
            }

            var results = profiling.SnapshotResults!;
            Log.Information(
                $"Analysis took {results.Time.TotalMilliseconds:N2}ms - " +
                $"Memory was adjusted by {results.Memory:N0} bytes");
        }
        catch (Exception ex)
        {
            App.Current.ExceptionListener.HandleException(ex, "Analysis failed");
            AnalysisFailed?.Invoke(new(ex));
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
