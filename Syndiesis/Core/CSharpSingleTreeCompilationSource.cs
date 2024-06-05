using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Threading;

namespace Syndiesis.Core;

public sealed class CSharpSingleTreeCompilationSource
    : BaseSingleTreeCompilationSource<CSharpCompilation>
{
    public override string LanguageName => LanguageNames.CSharp;

    protected override CSharpCompilation CreateCompilation()
    {
        return CSharpCompilation.Create("Syndiesis.UserSource");
    }

    protected override SyntaxTree ParseTree(string source, CancellationToken cancellationToken)
    {
        return CSharpSyntaxTree.ParseText(source, cancellationToken: cancellationToken);
    }
}
