using Syndiesis.Core.DisplayAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Syndiesis.Core;

public abstract class BaseAnalysisExecution(SingleTreeCompilationSource compilationSource)
{
    public AnalysisNodeCreationOptions NodeLineOptions { get; set; } = new();
    public SingleTreeCompilationSource CompilationSource { get; } = compilationSource;

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

        CompilationSource.SetSource(source, token);
        if (token.IsCancellationRequested)
            return Task.FromCanceled<AnalysisResult>(token);

        return ExecuteCore(token);
    }

    protected abstract Task<AnalysisResult> ExecuteCore(CancellationToken token);
}
