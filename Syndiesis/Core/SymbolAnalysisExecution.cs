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
        var creator = container.SymbolCreator;

        if (token.IsCancellationRequested)
            return Cancelled();

        var compilation = CompilationSource.Compilation;
        var assemblySymbol = compilation.Assembly;

        if (token.IsCancellationRequested)
            return Cancelled();

        var rootNode = creator.CreateRootViewNode(assemblySymbol!, default);
        var result = new SymbolAnalysisResult(rootNode!);
        return Task.FromResult<AnalysisResult>(result);
    }
}
