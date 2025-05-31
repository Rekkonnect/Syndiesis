using Microsoft.CodeAnalysis;

namespace Syndiesis.Core;

public interface ISingleTreeCompilationSource
{
    public bool DiagnosticsUnavailable { get; }

    public string LanguageName { get; }

    public Compilation Compilation { get; }
    public SyntaxTree? Tree { get; }
    public SemanticModel? SemanticModel { get; }

    public RoslynLanguageVersion LanguageVersion { get; }

    public DiagnosticsCollection GetDiagnostics();

    public void AdjustLanguageVersion(RoslynLanguageVersion version);

    public void SetSource(string source, CancellationToken cancellationToken);
}
