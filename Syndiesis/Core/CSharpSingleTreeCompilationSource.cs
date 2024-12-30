using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Threading;

namespace Syndiesis.Core;

public sealed class CSharpSingleTreeCompilationSource
    : BaseSingleTreeCompilationSource<CSharpCompilation, CSharpParseOptions>
{
    public override string LanguageName => LanguageNames.CSharp;

    public override RoslynLanguageVersion LanguageVersion => new(ParseOptions.LanguageVersion);

    protected override CSharpParseOptions CreateDefaultParseOptions()
    {
        return CSharpParseOptions.Default
            .WithLanguageVersion(
                Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp13);
    }

    protected override void AdjustLanguageVersionCore(RoslynLanguageVersion version)
    {
        var csVersion = version.CSharpVersion;
        ParseOptions = ParseOptions.WithLanguageVersion(csVersion);
    }

    protected override CSharpCompilation CreateCompilation()
    {
        var options = new CSharpCompilationOptions(OutputKind.NetModule)
            .WithAllowUnsafe(true);
        return CSharpCompilation
            .Create("Syndiesis.UserSource")
            .WithOptions(options);
    }

    protected override SyntaxTree ParseTree(string source, CancellationToken cancellationToken)
    {
        return CSharpSyntaxTree.ParseText(
            source,
            options: ParseOptions,
            cancellationToken: cancellationToken);
    }
}
