using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Garyon.Objects;
using Syndiesis.Updating;
using System.ComponentModel;
using Updatum;

namespace Syndiesis.Controls.Updating;

public partial class UpdatePopup : UserControl
{
    // TODO: This is a temporary solution to preserve the state of the button
    private bool _needsDownloading = false;

    public UpdatePopup()
    {
        InitializeComponent();
        InitializeEvents();
    }

    private void InitializeEvents()
    {
        var manager = Singleton<UpdateManager>.Instance;
        manager.UpdaterPropertyChanged += HandlePropertyChanged;
        mainButton.Click += OnUpdateInformationButtonClicked;
    }

    private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(InvalidateArrange);
    }

    protected override void ArrangeCore(Rect finalRect)
    {
        UpdateTexts();
        base.ArrangeCore(finalRect);
    }

    private void UpdateTexts()
    {
        var manager = Singleton<UpdateManager>.Instance;
        var downloadProgress = manager.DownloadProgress;
        var updateState = manager.UpdateState;
        var hasPendingUpdate = manager.HasPendingUpdate;

        buttonText.Text = GetText();

        string GetText()
        {
            var progress = downloadProgress?.Progress ?? 0;
            return updateState switch
            {
                UpdatumState.InstallingUpdate => "Installing update",
                UpdatumState.DownloadingUpdate => $"Downloading update: {progress:P1}",
                UpdatumState.CheckingForUpdate => "Checking for update",
                _ => hasPendingUpdate switch
                {
                    true => "Pending update",
                    false => "Up-to-date",
                }
            };
        }
    }

    private void OnUpdateInformationButtonClicked(object? sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(HandleMainButtonClick);
    }

    private void HandleMainButtonClick()
    {
        var manager = Singleton<UpdateManager>.Instance;
        if (_needsDownloading)
        {
            Task.Run(manager.EnsureUpdateDownloaded);
        }
        else
        {
            Task.Run(manager.InstallDownloadedUpdate);
        }
    }
}
