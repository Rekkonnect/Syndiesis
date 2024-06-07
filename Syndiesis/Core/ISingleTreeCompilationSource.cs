using Microsoft.CodeAnalysis;
using System.Threading;

namespace Syndiesis.Core;

public interface ISingleTreeCompilationSource
{
    public string LanguageName { get; }

    public Compilation Compilation { get; }
    public SyntaxTree? Tree { get; }
    public SemanticModel? SemanticModel { get; }

    public void AdjustLanguageVersion(RoslynLanguageVersion version);

    public void SetSource(string source, CancellationToken cancellationToken);
}
