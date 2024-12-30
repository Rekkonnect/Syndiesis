using System;
using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class HybridLanguageSymbolItemInlinesCreatorContainer
{
    private readonly CSharpSymbolItemInlinesCreatorContainer _csharp = new();
    private readonly VisualBasicSymbolItemInlinesCreatorContainer _vb = new();

    public BaseSymbolItemInlinesCreatorContainer ContainerForLanguage(string languageName)
    {
        return languageName switch
        {
            LanguageNames.CSharp => _csharp,
            LanguageNames.VisualBasic => _vb,
            _ => throw new ArgumentException("Unknown language requested"),
        };
    }
}
