using Avalonia.Controls.Documents;
using Garyon.Objects;
using Syndiesis.Controls.Inlines;
using Syndiesis.Updating;
using Syndiesis.Utilities;

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
        var updatableVersionInfo = GetUpdatableVersion();
        var displayedVersion = updatableVersionInfo.LatestDisplayedVersion;
        var sha = displayedVersion.CommitSha?.Short;

        const uint noUpdateTextColor = 0xFFB8E3E5;
        const uint updateTextColor = 0xFFB6E5AC;
        const uint labelTextColor = 0xFF667E80;

        bool hasUpdate = updatableVersionInfo.HasUpdate;
        uint textColor = hasUpdate
            ? updateTextColor
            : noUpdateTextColor;

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
                    new Run(displayedVersion.Version)
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

        updateVersionHeaderText.GroupedRunInlines = new(groups);

        updatesHeaderText.Text = hasUpdate switch
        {
            true => "Update available",
            false => "Updates",
        };
    }

    private static UpdatableVersionInformation GetUpdatableVersion()
    {
        var thisVersion = App.Current.AppInfo.InformationalVersion;
        var manager = Singleton<UpdateManager>.Instance;
        var updateVersion = manager.AvailableUpdateVersion;
        return new(thisVersion, updateVersion);
    }

    private void InitializeIcons()
    {
        closeButton.PathData = App.Current.ResourceManager.CloseIconGeometry;
    }

    private sealed record UpdatableVersionInformation(
        InformationalVersion CurrentVersion,
        InformationalVersion? UpdateVersion)
    {
        public bool HasUpdate => UpdateVersion is not null;

        public InformationalVersion LatestDisplayedVersion
            => UpdateVersion ?? CurrentVersion;
    }
}
