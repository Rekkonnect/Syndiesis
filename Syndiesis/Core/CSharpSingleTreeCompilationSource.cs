using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Threading;

namespace Syndiesis.Core;

public sealed class CSharpSingleTreeCompilationSource
    : BaseSingleTreeCompilationSource<CSharpCompilation, CSharpParseOptions>
{
    public override string LanguageName => LanguageNames.CSharp;

    protected override CSharpParseOptions CreateDefaultParseOptions()
    {
        return CSharpParseOptions.Default;
    }

    public override void AdjustLanguageVersion(RoslynLanguageVersion version)
    {
        var csVersion = version.CSharpVersion;
        ParseOptions = ParseOptions.WithLanguageVersion(csVersion);
    }

    protected override CSharpCompilation CreateCompilation()
    {
        return CSharpCompilation.Create("Syndiesis.UserSource");
    }

    protected override SyntaxTree ParseTree(string source, CancellationToken cancellationToken)
    {
        return CSharpSyntaxTree.ParseText(
            source,
            options: ParseOptions,
            cancellationToken: cancellationToken);
    }
}
