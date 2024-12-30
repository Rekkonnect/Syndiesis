using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using System.Threading;

namespace Syndiesis.Core;

public sealed class VisualBasicSingleTreeCompilationSource
    : BaseSingleTreeCompilationSource<VisualBasicCompilation, VisualBasicParseOptions>
{
    public override string LanguageName => LanguageNames.VisualBasic;

    public override RoslynLanguageVersion LanguageVersion => new(ParseOptions.LanguageVersion);

    protected override VisualBasicParseOptions CreateDefaultParseOptions()
    {
        return VisualBasicParseOptions.Default
            .WithLanguageVersion(
                Microsoft.CodeAnalysis.VisualBasic.LanguageVersion.VisualBasic16_9);
    }

    protected override void AdjustLanguageVersionCore(RoslynLanguageVersion version)
    {
        var vbVersion = version.VisualBasicVersion;
        ParseOptions = ParseOptions.WithLanguageVersion(vbVersion);
    }

    protected override VisualBasicCompilation CreateCompilation()
    {
        var options = new VisualBasicCompilationOptions(OutputKind.NetModule);
        return VisualBasicCompilation
            .Create("SymVBiosis.UserSource")
            .WithOptions(options);
    }

    protected override SyntaxTree ParseTree(string source, CancellationToken cancellationToken)
    {
        return VisualBasicSyntaxTree.ParseText(
            source,
            options: ParseOptions,
            cancellationToken: cancellationToken);
    }
}
