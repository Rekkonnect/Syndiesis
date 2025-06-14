﻿using Syndiesis.Core.DisplayAnalysis;

namespace Syndiesis.Core;

public class SyntaxNodeAnalysisExecution(HybridSingleTreeCompilationSource compilationSource)
    : BaseAnalysisExecution(compilationSource)
{
    protected override Task<AnalysisResult> ExecuteCore(
        CancellationToken token)
    {
        var container = CreateCreatorContainer();
        var creator = container.SyntaxCreator;

        var syntaxTree = CompilationSource.CurrentSource.Tree!;

        UIBuilder.AnalysisTreeListNode root;

        if (CreationOptions.ShowSyntaxTreeRootNode)
        {
            root = creator.CreateRootTree(syntaxTree, null as IDisplayValueSource);
            if (token.IsCancellationRequested)
                return Cancelled();
        }
        else
        {
            var treeRoot = syntaxTree.GetRoot(token);
            if (token.IsCancellationRequested)
                return Cancelled();

            root = creator.CreateRootNode(treeRoot, null as IDisplayValueSource);
            if (token.IsCancellationRequested)
                return Cancelled();
        }

        var result = new SyntaxNodeAnalysisResult(root);
        return Task.FromResult<AnalysisResult>(result);
    }
}
