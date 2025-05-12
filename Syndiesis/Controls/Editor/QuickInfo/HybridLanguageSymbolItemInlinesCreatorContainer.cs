using Microsoft.CodeAnalysis;
using System;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class HybridLanguageSymbolItemInlinesCreatorContainer
{
    private readonly CSharpSymbolInlinesRootCreatorContainer _csharp = new();
    private readonly VisualBasicSymbolInlinesRootCreatorContainer _vb = new();

    public ISymbolInlinesRootCreatorContainer ContainerForLanguage(string languageName)
    {
        return languageName switch
        {
            LanguageNames.CSharp => _csharp,
            LanguageNames.VisualBasic => _vb,
            _ => throw new ArgumentException("Unknown language requested"),
        };
    }
}
