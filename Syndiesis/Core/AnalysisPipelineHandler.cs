using Avalonia.Threading;
using AvaloniaEdit.Document;
using Garyon.Mechanisms;
using Garyon.Objects;
using Serilog;
using Syndiesis.Utilities;
using System;
using System.Threading.Tasks;

namespace Syndiesis.Core;

public class AnalysisPipelineHandler
{
    private readonly CancellationTokenFactory _analysisCancellationTokenFactory = new();

    private readonly Delayer _delayer = new();

    private ITextSource? _textSource = null;

    private volatile int _ignoredInputDelayTimes = 0;

    public TimeSpan UserInputDelay { get; set; } = AppSettings.Instance.UserInputDelay;

    public BaseAnalysisExecution? AnalysisExecution { get; set; }

    public bool IsWaiting => _delayer.IsWaiting;

    public event Action? AnalysisRequested;
    public event Action? AnalysisBegun;
    public event Action<AnalysisResult>? AnalysisCompleted;
    public event Action<FailedAnalysisResult>? AnalysisFailed;

    public void IgnoreInputDelayOnce()
    {
        _ignoredInputDelayTimes++;
    }

    public async Task ForceAnalysis(ITextSource source)
    {
        _textSource = source;
        await ForceAnalysis();
    }

    public async Task ForceAnalysis()
    {
        _analysisCancellationTokenFactory.Cancel();
        await PerformAnalysis()
            .ConfigureAwait(false);
    }

    public void InitiateAnalysis(ITextSource source)
    {
        if (AnalysisExecution is null)
            return;

        _textSource = source;
        if (_delayer.IsWaiting)
        {
            SetRequestedDelay();
            return;
        }

        _analysisCancellationTokenFactory.Cancel();

        Task.Run(PerformAnalysis);
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
                // this performs a check on TextDocument so we dispatch the retrieval of the source
                var source = Dispatcher.UIThread.Invoke(() => _textSource?.Text);
                var result = await AnalysisExecution.Execute(source, token);
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
            App.Current?.ExceptionListener.HandleException(ex, "Analysis failed");
            AnalysisFailed?.Invoke(new(ex));
        }
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
