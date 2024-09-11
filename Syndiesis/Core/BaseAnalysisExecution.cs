using Garyon.Objects;
using Syndiesis.Core.DisplayAnalysis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Syndiesis.Core;

public abstract class BaseAnalysisExecution(HybridSingleTreeCompilationSource compilationSource)
{
    public AnalysisNodeCreationOptions CreationOptions => AppSettings.Instance.NodeLineOptions;
    public HybridSingleTreeCompilationSource CompilationSource { get; } = compilationSource;

    protected BaseAnalysisNodeCreatorContainer CreateCreatorContainer()
    {
        var source = CompilationSource.CurrentSource;
        return BaseAnalysisNodeCreatorContainer.CreateForLanguage(source.LanguageName);
    }

    public Task<AnalysisResult> ExecuteForCurrentCompilation(CancellationToken token)
    {
        return ExecuteCore(token);
    }

    public Task<AnalysisResult> Execute(
        string? source,
        CancellationToken token)
    {
        if (source is null)
        {
            return ExecuteForCurrentCompilation(token);
        }

        try
        {
            CompilationSource.SetSource(source, token);
        }
        catch (OperationCanceledException cancellationException)
        {
            App.Current.ExceptionListener.HandleException(
                cancellationException,
                "Cancelled analysis execution during parsing of the source");
            return Cancelled();
        }
        if (token.IsCancellationRequested)
            return Cancelled();

        return ExecuteCore(token);
    }

    protected abstract Task<AnalysisResult> ExecuteCore(CancellationToken token);

    protected static Task<AnalysisResult> Cancelled()
    {
        return Task.FromResult<AnalysisResult>(Singleton<CancelledAnalysisResult>.Instance);
    }
}
