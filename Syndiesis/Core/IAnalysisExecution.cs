using Syndiesis.Core.DisplayAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Syndiesis.Core;

public interface IAnalysisExecution
{
    public AnalysisNodeCreationOptions NodeLineOptions { get; set; }

    public Task<AnalysisResult> Execute(
        string source,
        CancellationToken token);
}
