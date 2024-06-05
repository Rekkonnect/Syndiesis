using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using System.Threading;

namespace Syndiesis.Core;

public sealed class VisualBasicSingleTreeCompilationSource
    : BaseSingleTreeCompilationSource<VisualBasicCompilation>
{
    public override string LanguageName => LanguageNames.VisualBasic;

    protected override VisualBasicCompilation CreateCompilation()
    {
        return VisualBasicCompilation.Create("SymVBiosis.UserSource");
    }

    protected override SyntaxTree ParseTree(string source, CancellationToken cancellationToken)
    {
        return VisualBasicSyntaxTree.ParseText(source, cancellationToken: cancellationToken);
    }
}
