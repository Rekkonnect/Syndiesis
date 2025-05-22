using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Media;
using Garyon.Objects;
using Syndiesis.Controls.Inlines;
using Syndiesis.Controls.Toast;
using Syndiesis.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Syndiesis.Controls.Updating;

public partial class UpdateTopBar : UserControl
{
    private Run _versionRun;
    private Run _commitRun;

    private CancellationTokenFactory _pulseLineCancellationTokenFactory = new();

    public UpdateTopBar()
    {
        InitializeComponent();
        InitializeRuns();
        InitializeEvents();
    }

    private void InitializeEvents()
    {
        AddPointerHandlers(linePulseRectangle);
        AddPointerHandlers(contentStackPanel);
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

    [MemberNotNull(nameof(_versionRun))]
    [MemberNotNull(nameof(_commitRun))]
    private void InitializeRuns()
    {
        var infoVersion = App.Current.AppInfo.InformationalVersion;
        var version = infoVersion.Version;
        var sha = infoVersion.CommitSha?[..7];

        var groups = new RunOrGrouped[]
        {
            new ComplexGroupedRunInline(
            [
                new Run("v")
                {
                    FontSize = 16,
                    Foreground = new SolidColorBrush(0xFF808080),
                },

                new SingleRunInline(
                    _versionRun = new Run(version)
                    {
                        FontSize = 18,
                    }
                ),
            ]),

            new Run("  "),

            new ComplexGroupedRunInline(
            [
                new Run("commit ")
                {
                    FontSize = 14,
                    Foreground = new SolidColorBrush(0xFF808080),
                },

                new SingleRunInline(
                    _commitRun = new Run(sha)
                    {
                        FontSize = 18,
                    }
                ),
            ]),

#if DEBUG
            new Run("   "),

            new SingleRunInline(
                new Run("[DEBUG]")
                {
                    FontSize = 18,
                    Foreground = new SolidColorBrush(0xFF00A0AA),
                }
            ),
#endif
        };

        if (sha is null)
        {
            _commitRun.Foreground = new SolidColorBrush(0xFF440015);
            _commitRun.Text = "[MISSING]";
        }

        headerText.GroupedRunInlines = new(groups);
    }
}
