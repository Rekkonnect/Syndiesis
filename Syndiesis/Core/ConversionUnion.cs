using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

using CSharpConversion = Microsoft.CodeAnalysis.CSharp.Conversion;
using VisualBasicConversion = Microsoft.CodeAnalysis.VisualBasic.Conversion;

namespace Syndiesis.Core;

public sealed record ConversionUnion(
    CSharpConversion CSharpConversion,
    VisualBasicConversion VisualBasicConversion,
    string LanguageName)
{
    public static readonly ConversionUnion None = new(default, default, string.Empty);

    public object? AppliedConversion
    {
        get
        {
            return LanguageName switch
            {
                LanguageNames.CSharp => CSharpConversion,
                LanguageNames.VisualBasic => VisualBasicConversion,
                _ => null,
            };
        }
    }

    public CommonConversion CommonConversion
    {
        get
        {
            return LanguageName switch
            {
                LanguageNames.CSharp => CSharpConversion.ToCommonConversion(),
                LanguageNames.VisualBasic => VisualBasicConversion.ToCommonConversion(),
                _ => default,
            };
        }
    }

    public ConversionUnion(CSharpConversion cSharpConversion)
        : this(cSharpConversion, default, LanguageNames.CSharp)
    {
    }

    public ConversionUnion(VisualBasicConversion vbConversion)
        : this(default, vbConversion, LanguageNames.VisualBasic)
    {
    }

    public static implicit operator ConversionUnion(
        CSharpConversion cSharpConversion)
        => new(cSharpConversion);

    public static implicit operator ConversionUnion(
        VisualBasicConversion vbConversion)
        => new(vbConversion);
}
