using Microsoft.CodeAnalysis.CSharp.Syntax;
using Serilog;
using Syndiesis.Utilities;
using System;
using System.Threading.Tasks;

namespace Syndiesis.Core;

public class AnalysisPipelineHandler
{
    private readonly CancellationTokenFactory _analysisCancellationTokenFactory = new();

    private readonly Delayer _delayer = new();

    private string? _pendingSource = null;

    private volatile int _ignoredInputDelayTimes = 0;

    public TimeSpan UserInputDelay { get; set; } = AppSettings.Instance.UserInputDelay;

    public BaseAnalysisExecution? AnalysisExecution { get; set; }

    public event Action? AnalysisRequested;
    public event Action? AnalysisBegun;
    public event Action<AnalysisResult>? AnalysisCompleted;
    public event Action<FailedAnalysisResult>? AnalysisFailed;

    public void IgnoreInputDelayOnce()
    {
        _ignoredInputDelayTimes++;
    }

    public async Task ForceAnalysis()
    {
        _analysisCancellationTokenFactory.Cancel();
        await PerformAnalysis()
            .ConfigureAwait(false);
    }

    public void InitiateAnalysis(string source)
    {
        if (AnalysisExecution is null)
            return;

        _pendingSource = source;
        if (_delayer.IsWaiting)
        {
            SetRequestedDelay();
            return;
        }

        _analysisCancellationTokenFactory.Cancel();

        _ = PerformAnalysis()
            .ConfigureAwait(false);
    }

    private async Task PerformAnalysis()
    {
        var token = _analysisCancellationTokenFactory.CurrentToken;

        AnalysisRequested?.Invoke();

        SetRequestedDelay();
        await _delayer.WaitUnblock(token);
        if (token.IsCancellationRequested)
            return;

        AnalysisBegun?.Invoke();

        try
        {
            Log.Information($"Began analysis using {AnalysisExecution!.GetType()}");
            var profiling = new SimpleProfiling();
            using (profiling.BeginProcess())
            {
                var result = await AnalysisExecution.Execute(_pendingSource, token);
                if (token.IsCancellationRequested)
                {
                    return;
                }
                if (!result.Failed)
                {
                    AnalysisCompleted!.Invoke(result);
                }
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
        _pendingSource = null;
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
