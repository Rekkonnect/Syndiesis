using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using System.Threading;

namespace Syndiesis.Core;

public sealed class VisualBasicSingleTreeCompilationSource
    : BaseSingleTreeCompilationSource<VisualBasicCompilation, VisualBasicParseOptions>
{
    public override string LanguageName => LanguageNames.VisualBasic;

    protected override VisualBasicParseOptions CreateDefaultParseOptions()
    {
        return VisualBasicParseOptions.Default;
    }

    public override void AdjustLanguageVersion(RoslynLanguageVersion version)
    {
        var vbVersion = (LanguageVersion)version.RawVersionValue;
        ParseOptions = ParseOptions.WithLanguageVersion(vbVersion);
    }

    protected override VisualBasicCompilation CreateCompilation()
    {
        return VisualBasicCompilation.Create("SymVBiosis.UserSource");
    }

    protected override SyntaxTree ParseTree(string source, CancellationToken cancellationToken)
    {
        return VisualBasicSyntaxTree.ParseText(
            source,
            options: ParseOptions,
            cancellationToken: cancellationToken);
    }
}
