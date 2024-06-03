using Syndiesis.Core.DisplayAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Syndiesis.Core;

public class SyntaxNodeAnalysisExecution(CSharpSingleTreeCompilationSource compilationSource)
    : BaseAnalysisExecution(compilationSource)
{
    protected override Task<AnalysisResult> ExecuteCore(
        CancellationToken token)
    {
        var container = CreateCreatorContainer();
        var creator = container.SyntaxCreator;

        var syntaxTree = CompilationSource.Tree!;

        UIBuilder.AnalysisTreeListNode root;

        if (CreationOptions.ShowSyntaxTreeRootNode)
        {
            root = creator.CreateRootTree(syntaxTree);
            if (token.IsCancellationRequested)
                return Cancelled();
        }
        else
        {
            var treeRoot = syntaxTree.GetRoot(token);
            if (token.IsCancellationRequested)
                return Cancelled();

            root = creator.CreateRootNode(treeRoot);
            if (token.IsCancellationRequested)
                return Cancelled();
        }

        var result = new SyntaxNodeAnalysisResult(root);
        return Task.FromResult<AnalysisResult>(result);
    }
}
