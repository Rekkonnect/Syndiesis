using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Syndiesis.Core;
using System;

using CSharpVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion;
using VisualBasicVersion = Microsoft.CodeAnalysis.VisualBasic.LanguageVersion;

namespace Syndiesis.Controls;

public partial class LanguageVersionDropDownItems : UserControl
{
    public event EventHandler<RoutedEventArgs>? ItemClicked;

    public LanguageVersionDropDownItems()
    {
        InitializeComponent();
        InitializeValidLanguageVersions();
    }

    private static readonly SolidColorBrush csVersionForeground = new(0xFF7BB0A6);
    private static readonly SolidColorBrush vbVersionForeground = new(0xFF7BA6B0);

    private void InitializeValidLanguageVersions()
    {
        var csPreviewItem = CreateItem(new(CSharpVersion.Preview));
        csPreviewItem.TextBlock.Foreground = csVersionForeground;

        csVersionsGrid.Children.Add(csPreviewItem);
        Grid.SetRow(csPreviewItem, 0);
        Grid.SetColumn(csPreviewItem, 0);
        Grid.SetColumnSpan(csPreviewItem, 2);

        // Just for spacing purposes
        var vbPreviewItem = CreateItem(new(CSharpVersion.Preview));
        vbPreviewItem.Opacity = 0;

        vbVersionsGrid.Children.Add(vbPreviewItem);
        Grid.SetRow(vbPreviewItem, 0);
        Grid.SetColumn(vbPreviewItem, 0);
        Grid.SetColumnSpan(vbPreviewItem, 2);

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
            var item = CreateCSVersionItem(version);
            csVersionsLeftPanel.Children.Add(item);
        }

        foreach (var version in csVersionsRight)
        {
            var item = CreateCSVersionItem(version);
            csVersionsRightPanel.Children.Add(item);
        }

        LanguageVersionDropDownItem CreateCSVersionItem(CSharpVersion version)
        {
            var item = CreateItem(new(version));
            item.TextBlock.Foreground = csVersionForeground;
            return item;
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
            var item = CreateVBVersionItem(version);
            vbVersionsLeftPanel.Children.Add(item);
        }

        foreach (var version in vbVersionsRight)
        {
            var item = CreateVBVersionItem(version);
            vbVersionsRightPanel.Children.Add(item);
        }

        LanguageVersionDropDownItem CreateVBVersionItem(VisualBasicVersion version)
        {
            var item = CreateItem(new(version));
            item.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
            item.TextBlock.TextAlignment = TextAlignment.Right;
            item.TextBlock.Foreground = vbVersionForeground;
            return item;
        }
    }

    private void HandleItemClicked(LanguageVersionDropDownItem? sender, RoutedEventArgs e)
    {
        ItemClicked?.Invoke(sender, e);
    }

    private LanguageVersionDropDownItem CreateItem(RoslynLanguageVersion version)
    {
        var item = new LanguageVersionDropDownItem(version);
        item.Clicked += (_, e) => HandleItemClicked(item, e);
        return item;
    }
}
