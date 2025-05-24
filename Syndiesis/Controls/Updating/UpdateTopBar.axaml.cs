using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Garyon.Objects;
using Syndiesis.Controls.Inlines;
using Syndiesis.Controls.Toast;
using Syndiesis.Core;
using Syndiesis.Updating;
using Syndiesis.Utilities;
using System;

namespace Syndiesis.Controls.Updating;

public partial class UpdateTopBar : UserControl
{
    private Run? _versionRun;
    private Run? _commitRun;

    private CancellationTokenFactory _pulseLineCancellationTokenFactory = new();

    public UpdateTopBar()
    {
        InitializeComponent();
        InitializeRuns();
        InitializeEvents();
        InitializeIcons();
    }

    private void InitializeEvents()
    {
        AddPointerHandlers(linePulseRectangle);
        AddPointerHandlers(contentStackPanel);
        closeButton.Click += OnCloseButtonClicked;
    }

    private void OnCloseButtonClicked(object? sender, RoutedEventArgs e)
    {
        // DO SOMETHING
        var popup = this.NearestAncestorOfType<UpdatePopup>();
        if (popup is null)
        {
            // TODO: LOG
            return;
        }

        var container = popup.NearestAncestorOfType<PopupDisplayContainer>();
        Dispatcher.UIThread.InvokeAsync(container.Hide);
    }

    private void AddPointerHandlers(Control control)
    {
        control.PointerPressed += HandleLineTapped;
    }

    private void HandleLineTapped(object? sender, PointerEventArgs e)
    {
        var pointer = e.GetCurrentPoint(this);
        if (pointer.Properties.IsLeftButtonPressed)
        {
            switch (e.KeyModifiers.NormalizeByPlatform())
            {
                case KeyModifiers.Control:
                    CopyEntireLine();
                    break;
            }
        }
    }

    private void CopyEntireLine()
    {
        var text = headerText.Inlines!.Text;
        _ = this.SetClipboardTextAsync(text)
            .ConfigureAwait(false);
        PulseCopiedLine();

        var toastContainer = ToastNotificationContainer.GetFromOuterMainViewContainer(this);
        if (toastContainer is not null)
        {
            var popupContent = $"""
                Copied entire line content:
                {text}
                """;
            _ = CommonToastNotifications.ShowClassicMain(
                toastContainer,
                popupContent,
                TimeSpan.FromSeconds(2));
        }
    }

    private void PulseCopiedLine()
    {
        _pulseLineCancellationTokenFactory.Cancel();
        var animation = Animations.CreateOpacityPulseAnimation(linePulseRectangle, 1, OpacityProperty);
        animation.Duration = TimeSpan.FromMilliseconds(750);
        animation.Easing = Singleton<CubicEaseOut>.Instance;
        _ = animation.RunAsync(linePulseRectangle, _pulseLineCancellationTokenFactory.CurrentToken);
    }

    private void InitializeRuns()
    {
        var manager = Singleton<UpdateManager>.Instance;
        var release = manager.Release;

        if (release is null)
        {
            return;
        }

        var version = ParseVersion(manager.LatestVersionString)?.ToString() ?? "?.?.?";
        string? sha = manager.LatestReleaseCommit?.Sha.ShortCommitSha();

        const uint textColor = 0xFFB8E3E5;
        const uint labelTextColor = 0xFF667E80;

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
                    _versionRun = new Run(version)
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
                    _commitRun = new Run(sha)
                    {
                        FontSize = 18,
                        Foreground = new SolidColorBrush(textColor),
                    }
                ),
            ]),
        };

        if (sha is null)
        {
            _commitRun.Foreground = new SolidColorBrush(0xFF440015);
            _commitRun.Text = "[MISSING]";
        }

        headerText.GroupedRunInlines = new(groups);
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
