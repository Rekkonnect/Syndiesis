using Microsoft.CodeAnalysis.CSharp;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Core.DisplayAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Syndiesis.Core;

public class SyntaxNodeAnalysisExecution : IAnalysisExecution
{
    public AnalysisNodeCreationOptions NodeLineOptions { get; set; } = new();

    public Task<AnalysisResult> Execute(
        string source,
        CancellationToken token)
    {
        var creator = new SyntaxAnalysisNodeCreator(NodeLineOptions);

        var syntaxTree = CSharpSyntaxTree.ParseText(source, cancellationToken: token);
        if (token.IsCancellationRequested)
            return Task.FromCanceled<AnalysisResult>(token);

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
