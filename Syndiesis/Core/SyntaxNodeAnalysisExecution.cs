using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Core.DisplayAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Syndiesis.Core;

public class SyntaxNodeAnalysisExecution(SingleTreeCompilationSource compilationSource)
    : BaseAnalysisExecution(compilationSource)
{
    protected override Task<AnalysisResult> ExecuteCore(
        CancellationToken token)
    {
        var creator = new SyntaxAnalysisNodeCreator(NodeLineOptions);

        var syntaxTree = CompilationSource.Tree!;

        AnalysisTreeListNode root;

        if (NodeLineOptions.ShowSyntaxTreeRootNode)
        {
            root = creator.CreateRootTree(syntaxTree);
            if (token.IsCancellationRequested)
                return Task.FromCanceled<AnalysisResult>(token);
        }
        else
        {
            var treeRoot = syntaxTree.GetRoot(token);
            if (token.IsCancellationRequested)
                return Task.FromCanceled<AnalysisResult>(token);

            root = creator.CreateRootNode(treeRoot);
            if (token.IsCancellationRequested)
                return Task.FromCanceled<AnalysisResult>(token);
        }

        var result = new SyntaxNodeAnalysisResult(root);
        return Task.FromResult<AnalysisResult>(result);
    }
}
