using Garyon.Extensions;
using Garyon.Objects;
using Syndiesis.Updating;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Syndiesis.Controls.Updating;

public partial class UpdateProgressBar : UserControl
{
    private ColumnDistributor _progressBarColumnDistributor;

    public UpdateProgressBar()
    {
        InitializeComponent();
        InitializeEvents();
        InitializeDistribution();
        UpdateProgress();
    }

    [MemberNotNull(nameof(_progressBarColumnDistributor))]
    private void InitializeDistribution()
    {
        var columns = progressBarGrid.ColumnDefinitions;
        _progressBarColumnDistributor = new(
            columns[0], columns[1]);
    }

    private void InitializeEvents()
    {
        var updateManager = Singleton<UpdateManager>.Instance;
        updateManager.UpdaterPropertyChanged += HandlePropertyChanged;
    }

    private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(UpdateProgress);
    }

    private void UpdateProgress()
    {
        var manager = Singleton<UpdateManager>.Instance;
        var progress = manager.DownloadProgress;
        bool isDownloading = progress is not null;

        noUpdateInformationText.IsVisible = !isDownloading;
        downloadProgressTextGrid.IsVisible = isDownloading;

        if (!isDownloading)
        {
            return;
        }

        var progressRatio = progress!.Value.Progress;
        _progressBarColumnDistributor.SetProgressRatio(progressRatio);

        downloadedMegabytesText.Text = MegabyteString(progress!.Value.DownloadedBytes);
        updateMegabytesText.Text = MegabyteString(progress!.Value.TotalBytes.ZeroOrGreater());
    }

    private static string MegabyteString(long bytes)
    {
        const double megabyteSize = 1024 * 1024;
        var megabytes = bytes / megabyteSize;
        return megabytes.ToString("N2");
    }

    private sealed record ColumnDistributor(
        ColumnDefinition Progressed,
        ColumnDefinition Remaining)
    {
        public void SetProgressRatio(double ratio)
        {
            Progressed.Width = CreateRatioLength(ratio);
            Remaining.Width = CreateRatioLength(1 - ratio);
        }

        private static GridLength CreateRatioLength(double ratio)
        {
            return new(ratio, GridUnitType.Star);
        }
    }
}
