using Syndiesis.Core.DisplayAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Syndiesis.Core;

public class AttributeAnalysisExecution(HybridSingleTreeCompilationSource compilationSource)
    : BaseAnalysisExecution(compilationSource)
{
    protected override Task<AnalysisResult> ExecuteCore(
        CancellationToken token)
    {
        var container = CreateCreatorContainer();
        var creator = container.AttributeCreator;

        if (token.IsCancellationRequested)
            return Cancelled();

        var currentSource = CompilationSource.CurrentSource;
        var compilation = currentSource.Compilation;
        var tree = currentSource.Tree;
        var attributeTree = AttributeTree.FromTree(compilation, tree!, token);

        if (token.IsCancellationRequested)
            return Cancelled();

        var rootNode = creator.CreateRootAttributeTree(attributeTree!, null as IDisplayValueSource);
        var result = new AttributeAnalysisResult(rootNode);
        return Task.FromResult<AnalysisResult>(result);
    }
}
