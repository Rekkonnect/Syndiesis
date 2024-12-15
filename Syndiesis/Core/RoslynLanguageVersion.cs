using Microsoft.CodeAnalysis;

using CSharpVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion;
using VisualBasicVersion = Microsoft.CodeAnalysis.VisualBasic.LanguageVersion;

namespace Syndiesis.Core;

public readonly record struct RoslynLanguageVersion(string LanguageName, int RawVersionValue)
{
    public CSharpVersion CSharpVersion => (CSharpVersion)RawVersionValue;
    public VisualBasicVersion VisualBasicVersion => (VisualBasicVersion)RawVersionValue;

    public RoslynLanguageVersion(CSharpVersion version)
        : this(LanguageNames.CSharp, (int)version)
    { }

    public RoslynLanguageVersion(VisualBasicVersion version)
        : this(LanguageNames.VisualBasic, (int)version)
    { }

    public string DisplayVersionNumber()
    {
        return LanguageName switch
        {
            LanguageNames.CSharp => DisplayVersionNumber(CSharpVersion),
            LanguageNames.VisualBasic => DisplayVersionNumber(VisualBasicVersion),
            _ => "Other",
        };
    }

    public static string DisplayVersionNumber(CSharpVersion version)
    {
        return version switch
        {
            CSharpVersion.CSharp1 => "1.0",
            CSharpVersion.CSharp2 => "2.0",
            CSharpVersion.CSharp3 => "3.0",
            CSharpVersion.CSharp4 => "4.0",
            CSharpVersion.CSharp5 => "5.0",
            CSharpVersion.CSharp6 => "6.0",
            CSharpVersion.CSharp7 => "7.0",
            CSharpVersion.CSharp7_1 => "7.1",
            CSharpVersion.CSharp7_2 => "7.2",
            CSharpVersion.CSharp7_3 => "7.3",
            CSharpVersion.CSharp8 => "8.0",
            CSharpVersion.CSharp9 => "9.0",
            CSharpVersion.CSharp10 => "10.0",
            CSharpVersion.CSharp11 => "11.0",
            CSharpVersion.CSharp12 => "12.0",
            CSharpVersion.CSharp13 => "13.0",
            CSharpVersion.Preview => "Preview",

            _ => "Other",
        };
    }

    public static string DisplayVersionNumber(VisualBasicVersion version)
    {
        return version switch
        {
            VisualBasicVersion.VisualBasic9 => "9",
            VisualBasicVersion.VisualBasic10 => "10",
            VisualBasicVersion.VisualBasic11 => "11",
            VisualBasicVersion.VisualBasic12 => "12",
            VisualBasicVersion.VisualBasic14 => "14",
            VisualBasicVersion.VisualBasic15 => "15",
            VisualBasicVersion.VisualBasic15_3 => "15.3",
            VisualBasicVersion.VisualBasic15_5 => "15.5",
            VisualBasicVersion.VisualBasic16 => "16",
            VisualBasicVersion.VisualBasic16_9 => "16.9",

            _ => "Other",
        };
    }
}
