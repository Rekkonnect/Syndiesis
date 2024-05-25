using System.Threading;
using System.Threading.Tasks;

namespace Syndiesis.Core;

public class OperationAnalysisExecution(SingleTreeCompilationSource compilationSource)
    : BaseAnalysisExecution(compilationSource)
{
    protected override Task<AnalysisResult> ExecuteCore(
        CancellationToken token)
    {
        var container = CreateCreatorContainer();
        var creator = container.OperationCreator;

        if (token.IsCancellationRequested)
            return Cancelled();

        var compilation = CompilationSource.Compilation;
        var tree = CompilationSource.Tree;
        var operationTree = OperationTree.FromTree(compilation, tree!, token);

        if (token.IsCancellationRequested)
            return Cancelled();

        var rootNode = creator.CreateRootOperationTree(operationTree!, default);
        var result = new OperationAnalysisResult(rootNode);
        return Task.FromResult<AnalysisResult>(result);
    }
}
