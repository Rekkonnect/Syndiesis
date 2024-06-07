using Avalonia.Controls;
using System;

using CSharpVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion;
using VisualBasicVersion = Microsoft.CodeAnalysis.VisualBasic.LanguageVersion;

namespace Syndiesis.Controls;

public partial class LanguageVersionDropDownItems : UserControl
{
    public LanguageVersionDropDownItems()
    {
        InitializeComponent();
        InitializeValidLanguageVersions();
    }

    private void InitializeValidLanguageVersions()
    {
        var csPreviewItem = new LanguageVersionDropDownItem(
            new(CSharpVersion.Preview));
        csVersionsGrid.Children.Add(csPreviewItem);
        Grid.SetRow(csPreviewItem, 0);
        Grid.SetColumn(csPreviewItem, 0);

        // Just for spacing purposes
        var vbPreviewItem = new LanguageVersionDropDownItem(
            new(CSharpVersion.Preview))
        {
            Opacity = 0,
        };
        vbVersionsGrid.Children.Add(vbPreviewItem);
        Grid.SetRow(vbPreviewItem, 0);
        Grid.SetColumn(vbPreviewItem, 0);

        // C#

        ReadOnlySpan<CSharpVersion> csVersionsLeft =
        [
            CSharpVersion.CSharp12,
            CSharpVersion.CSharp11,
            CSharpVersion.CSharp10,
            CSharpVersion.CSharp9,
            CSharpVersion.CSharp8,
            CSharpVersion.CSharp7_3,
            CSharpVersion.CSharp7_2,
            CSharpVersion.CSharp7_1,
            CSharpVersion.CSharp7,
        ];

        ReadOnlySpan<CSharpVersion> csVersionsRight =
        [
            CSharpVersion.CSharp6,
            CSharpVersion.CSharp5,
            CSharpVersion.CSharp4,
            CSharpVersion.CSharp3,
            CSharpVersion.CSharp2,
            CSharpVersion.CSharp1,
        ];

        foreach (var version in csVersionsLeft)
        {
            var item = new LanguageVersionDropDownItem(new(version));
            csVersionsLeftPanel.Children.Add(item);
        }

        foreach (var version in csVersionsRight)
        {
            var item = new LanguageVersionDropDownItem(new(version));
            csVersionsRightPanel.Children.Add(item);
        }

        // VB

        ReadOnlySpan<VisualBasicVersion> vbVersionsLeft =
        [
            VisualBasicVersion.VisualBasic16_9,
            VisualBasicVersion.VisualBasic16,
            VisualBasicVersion.VisualBasic15_5,
            VisualBasicVersion.VisualBasic15_3,
            VisualBasicVersion.VisualBasic15,
        ];

        ReadOnlySpan<VisualBasicVersion> vbVersionsRight =
        [
            VisualBasicVersion.VisualBasic14,
            VisualBasicVersion.VisualBasic12,
            VisualBasicVersion.VisualBasic11,
            VisualBasicVersion.VisualBasic10,
            VisualBasicVersion.VisualBasic9,
        ];

        foreach (var version in vbVersionsLeft)
        {
            var item = new LanguageVersionDropDownItem(new(version))
            {
                TextAlignment = Avalonia.Media.TextAlignment.Right,
            };
            vbVersionsLeftPanel.Children.Add(item);
        }

        foreach (var version in vbVersionsRight)
        {
            var item = new LanguageVersionDropDownItem(new(version))
            {
                TextAlignment = Avalonia.Media.TextAlignment.Right,
            };
            vbVersionsRightPanel.Children.Add(item);
        }
    }
}
