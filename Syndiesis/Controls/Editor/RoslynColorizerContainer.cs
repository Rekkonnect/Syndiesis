using Microsoft.CodeAnalysis;
using Syndiesis.Core;

namespace Syndiesis.Controls.Editor;

public sealed class RoslynColorizerContainer(
    HybridSingleTreeCompilationSource compilationSource)
{
    public HybridSingleTreeCompilationSource CompilationSource { get; } = compilationSource;

    private readonly CSharpRoslynColorizer _csColorizer = new(compilationSource.CSharpSource);
    private readonly VisualBasicRoslynColorizer _vbColorizer = new(compilationSource.VisualBasicSource);

    public RoslynColorizer EffectiveColorizer
        => ColorizerForLanguage(CompilationSource.CurrentLanguageName);

    public bool Enabled
    {
        get => _csColorizer.Enabled;
        set
        {
            _csColorizer.Enabled = value;
            _vbColorizer.Enabled = value;
        }
    }

    private RoslynColorizer ColorizerForLanguage(string languageName)
    {
        return languageName switch
        {
            LanguageNames.CSharp => _csColorizer,
            LanguageNames.VisualBasic => _vbColorizer,
            _ => throw new ArgumentException("Invalid language name", nameof(languageName))
        };
    }
}
