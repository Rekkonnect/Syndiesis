using Syndiesis.Core.DisplayAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Syndiesis.Core;

public class OperationAnalysisExecution(SingleTreeCompilationSource compilationSource)
    : IAnalysisExecution
{
    public AnalysisNodeCreationOptions NodeLineOptions { get; set; } = new();
    public SingleTreeCompilationSource CompilationSource { get; } = compilationSource;

    public Task<AnalysisResult> Execute(
        string source,
        CancellationToken token)
    {
        var creator = new OperationsAnalysisNodeCreator(NodeLineOptions);

        CompilationSource.SetSource(source, token);
        if (token.IsCancellationRequested)
            return Task.FromCanceled<AnalysisResult>(token);

        var compilation = CompilationSource.Compilation;
        var tree = CompilationSource.Tree;
        var operationTree = OperationTree.FromTree(compilation, tree, token);

        if (token.IsCancellationRequested)
            return Task.FromCanceled<AnalysisResult>(token);

        var rootNode = creator.CreateRootOperationTree(operationTree, default);
        var result = new OperationAnalysisResult(rootNode);
        return Task.FromResult<AnalysisResult>(result);
    }
}
