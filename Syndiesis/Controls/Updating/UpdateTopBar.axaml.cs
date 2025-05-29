using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Garyon.Objects;
using Syndiesis.Controls.Inlines;
using Syndiesis.Core;
using Syndiesis.Updating;
using System;

namespace Syndiesis.Controls.Updating;

public partial class UpdateTopBar : UserControl
{
    public UpdateTopBar()
    {
        InitializeComponent();
        InitializeRuns();
        InitializeEvents();
        InitializeIcons();
    }

    private void InitializeEvents()
    {
        closeButton.Click += OnCloseButtonClicked;
        var updateManager = Singleton<UpdateManager>.Instance;
        updateManager.UpdaterStateChanged += HandleUpdaterStateChanged;
    }

    private void HandleUpdaterStateChanged(object? sender, UpdateManager.State e)
    {
        Dispatcher.UIThread.InvokeAsync(InitializeRuns);
    }

    private void OnCloseButtonClicked(object? sender, RoutedEventArgs e)
    {
        var popup = this.NearestAncestorOfType<UpdatePopup>();
        if (popup is null)
        {
            // TODO: LOG
            return;
        }

        var container = popup.NearestAncestorOfType<PopupDisplayContainer>();
        Dispatcher.UIThread.InvokeAsync(container.Hide);
    }

    private void InitializeRuns()
    {
        var manager = Singleton<UpdateManager>.Instance;

        var version = ParseVersion(manager.LatestVersionString)?.ToString() ?? "?.?.?";
        string? sha = manager.LatestReleaseCommit?.Sha.ShortCommitSha();

        const uint textColor = 0xFFB8E3E5;
        const uint labelTextColor = 0xFF667E80;

        Run commitRun;

        var groups = new RunOrGrouped[]
        {
            new ComplexGroupedRunInline(
            [
                new Run("v")
                {
                    FontSize = 16,
                    Foreground = new SolidColorBrush(labelTextColor),
                },

                new SingleRunInline(
                    new Run(version)
                    {
                        FontSize = 18,
                        Foreground = new SolidColorBrush(textColor),
                    }
                ),
            ]),

            new Run("      "),

            new ComplexGroupedRunInline(
            [
                new Run("commit ")
                {
                    FontSize = 14,
                    Foreground = new SolidColorBrush(labelTextColor),
                },

                new SingleRunInline(
                    commitRun = new Run(sha)
                    {
                        FontSize = 18,
                        Foreground = new SolidColorBrush(textColor),
                    }
                ),
            ]),
        };

        if (sha is null)
        {
            commitRun.Foreground = new SolidColorBrush(0xFF440015);
            commitRun.Text = "[MISSING]";
        }

        bool hasAvailableUpdate = manager.Release is not null;

        updateVersionHeaderText.GroupedRunInlines = new(groups);
        updateVersionHeaderText.IsVisible = hasAvailableUpdate;

        updatesHeaderText.Text = hasAvailableUpdate switch
        {
            true => "Update available",
            false => "Updates",
        };
    }

    private static Version? ParseVersion(string? tagName)
    {
        if (tagName is null)
            return null;

        ReadOnlySpan<char> tag = tagName;
        if (tag.StartsWith('v'))
        {
            tag = tag[1..];
        }

        bool parsed = Version.TryParse(tag, out var version);
        return version;
    }

    private void InitializeIcons()
    {
        closeButton.PathData = App.Current.ResourceManager.CloseIconGeometry;
    }
}
