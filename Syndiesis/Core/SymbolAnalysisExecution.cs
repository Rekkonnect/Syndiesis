using System.Threading;
using System.Threading.Tasks;

namespace Syndiesis.Core;

public class SymbolAnalysisExecution(SingleTreeCompilationSource compilationSource)
    : BaseAnalysisExecution(compilationSource)
{
    protected override Task<AnalysisResult> ExecuteCore(
        CancellationToken token)
    {
        var container = CreateCreatorContainer();
        var creator = container.SyntaxCreator;

        if (token.IsCancellationRequested)
            return Task.FromCanceled<AnalysisResult>(token);

        var compilation = CompilationSource.Compilation;
        var assemblySymbol = compilation.Assembly;

        if (token.IsCancellationRequested)
            return Task.FromCanceled<AnalysisResult>(token);

        var rootNode = creator.CreateRootViewNode(assemblySymbol!, default);
        var result = new SymbolAnalysisResult(rootNode!);
        return Task.FromResult<AnalysisResult>(result);
    }
}
