using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Garyon.Objects;
using Syndiesis.Updating;
using System.ComponentModel;
using System.Data;
using Updatum;

namespace Syndiesis.Controls.Updating;

public partial class UpdateInformationButton : UserControl
{
    public UpdateInformationButton()
    {
        InitializeComponent();
        InitializeEvents();
    }

    private void InitializeEvents()
    {
        var manager = Singleton<UpdateManager>.Instance;
        manager.UpdaterPropertyChanged += HandlePropertyChanged;
        button.Click += OnUpdateInformationButtonClicked;
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
        Dispatcher.UIThread.InvokeAsync(ShowUpdateInformationPopup);
    }

    private void ShowUpdateInformationPopup()
    {
        var container = PopupDisplayContainer.GetFromOuterMainViewContainer(this);
        if (container is null)
            return;

        container.Popup = new UpdatePopup();
        Dispatcher.UIThread.InvokeAsync(container.Show);
    }
}
