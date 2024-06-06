using Microsoft.CodeAnalysis;

using CSharpVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion;
using VisualBasicVersion = Microsoft.CodeAnalysis.VisualBasic.LanguageVersion;

namespace Syndiesis.Core;

public readonly record struct RoslynLanguageVersion(string LanguageName, int RawVersionValue)
{
    public RoslynLanguageVersion(CSharpVersion version)
        : this(LanguageNames.CSharp, (int)version)
    { }

    public RoslynLanguageVersion(VisualBasicVersion version)
        : this(LanguageNames.VisualBasic, (int)version)
    { }
}
