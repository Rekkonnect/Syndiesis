using Microsoft.CodeAnalysis;
using Syndiesis.Core;

namespace Syndiesis.Controls;

public partial class LanguageVersionDropDownEnvelope : UserControl
{
    public LanguageVersionDropDownEnvelope()
    {
        InitializeComponent();
        DisplayVersion(new(CSharpVersion.CSharp13));
    }

    public void DisplayVersion(RoslynLanguageVersion version)
    {
        var versionNumber = DisplayVersionNumber(version);
        languageText.Text = $"{version.LanguageName}  {versionNumber}";
    }

    private static string DisplayVersionNumber(RoslynLanguageVersion version)
    {
        switch (version.LanguageName)
        {
            case LanguageNames.CSharp:
                return RoslynLanguageVersion.DisplayVersionNumber(
                    version.CSharpVersion);

            case LanguageNames.VisualBasic:
                return RoslynLanguageVersion.DisplayVersionNumber(
                    version.VisualBasicVersion);

            default:
                return "Other";
        }
    }
}
