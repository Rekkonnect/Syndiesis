using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Microsoft.CodeAnalysis;
using Syndiesis.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using CSharpVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion;
using VisualBasicVersion = Microsoft.CodeAnalysis.VisualBasic.LanguageVersion;

namespace Syndiesis.Controls;

public partial class LanguageVersionDropDownItems : UserControl
{
    private static readonly SolidColorBrush csVersionForeground = new(0xFF7BB0A6);
    private static readonly SolidColorBrush vbVersionForeground = new(0xFF7BA6B0);
    private static readonly SolidColorBrush selectedButtonBackground = new(0x80909090);

    private readonly List<LanguageVersionDropDownItem> _csItems = new();
    private readonly List<LanguageVersionDropDownItem> _vbItems = new();

    public event EventHandler<RoutedEventArgs>? ItemClicked;

    public LanguageVersionDropDownItems()
    {
        InitializeComponent();
        InitializeValidLanguageVersions();
    }

    public void SetVersion(RoslynLanguageVersion version)
    {
        UncheckAll();
        var displayedVersion = version.DisplayVersionNumber();
        var item = ItemsForLanguage(version.LanguageName)
            .FirstOrDefault(item => item.TextBlock.Text == displayedVersion);
        Debug.Assert(item is not null, "the language cannot be out of the list");
        SetItemChecked(item);
    }

    private void UncheckAll()
    {
        foreach (var item in _csItems)
        {
            SetItemUnchecked(item);
        }

        foreach (var item in _vbItems)
        {
            SetItemUnchecked(item);
        }
    }

    private void SetItemChecked(LanguageVersionDropDownItem item)
    {
        item.Button.Background = selectedButtonBackground;
    }

    private void SetItemUnchecked(LanguageVersionDropDownItem item)
    {
        item.Button.Background = Brushes.Transparent;
    }

    private void InitializeValidLanguageVersions()
    {
        _csItems.Clear();
        _vbItems.Clear();

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
        var items = ItemsForLanguage(version.LanguageName);
        items.Add(item);
        item.Clicked += (_, e) => HandleItemClicked(item, e);
        return item;
    }

    private List<LanguageVersionDropDownItem> ItemsForLanguage(string languageName)
    {
        switch (languageName)
        {
            case LanguageNames.CSharp:
                return _csItems;
            case LanguageNames.VisualBasic:
                return _vbItems;

            default:
                throw RoslynExceptions.ThrowInvalidLanguageArgument(
                    languageName, nameof(languageName));
        }
    }
}
