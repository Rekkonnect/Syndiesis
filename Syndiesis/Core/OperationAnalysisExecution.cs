using Syndiesis.Core.DisplayAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Syndiesis.Core;

public class OperationAnalysisExecution(HybridSingleTreeCompilationSource compilationSource)
    : BaseAnalysisExecution(compilationSource)
{
    protected override Task<AnalysisResult> ExecuteCore(
        CancellationToken token)
    {
        var container = CreateCreatorContainer();
        var creator = container.OperationCreator;

        if (token.IsCancellationRequested)
            return Cancelled();

        var currentSource = CompilationSource.CurrentSource;
        var compilation = currentSource.Compilation;
        var tree = currentSource.Tree;
        var operationTree = OperationTree.FromTree(compilation, tree!, token);

        if (token.IsCancellationRequested)
            return Cancelled();

        var rootNode = creator.CreateRootOperationTree(operationTree!, null as IDisplayValueSource);
        var result = new OperationAnalysisResult(rootNode);
        return Task.FromResult<AnalysisResult>(result);
    }
}
