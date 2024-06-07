using Avalonia.Controls;
using Microsoft.CodeAnalysis;
using Syndiesis.Core;

namespace Syndiesis.Controls;

public partial class LanguageVersionDropDownEnvelope : UserControl
{
    public LanguageVersionDropDownEnvelope()
    {
        InitializeComponent();
    }

    public void DisplayVersion(RoslynLanguageVersion version)
    {
        languageNameRun.Text = version.LanguageName;
        versionRun.Text = DisplayVersionNumber(version);
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
