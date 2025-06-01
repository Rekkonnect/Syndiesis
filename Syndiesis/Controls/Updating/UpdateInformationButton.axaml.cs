using Garyon.Objects;
using Syndiesis.Updating;
using System.ComponentModel;

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
        manager.UpdaterStateChanged += HandleStateChanged;
        button.Click += OnUpdateInformationButtonClicked;
    }

    private void HandleStateChanged(object? sender, UpdateManager.State state)
    {
        if (AppSettings.Instance.UpdateOptions.AutoShowPopupOnAvailableUpdate)
        {
            if (state is UpdateManager.State.ReadyToInstall)
            {
                Dispatcher.UIThread.InvokeAsync(ShowUpdateInformationPopup);
            }
        }
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

        buttonText.Text = GetText();

        string GetText()
        {
            var progress = downloadProgress?.Progress ?? 0;
            return updateState switch
            {
                UpdateManager.State.Unchecked => "Check for updates",
                UpdateManager.State.Checking => "Checking for update",
                UpdateManager.State.UpToDate => "Up-to-date",
                UpdateManager.State.DiscoveredUpdate => "Pending update",
                UpdateManager.State.Downloading => $"Downloading update: {progress:P1}",
                UpdateManager.State.ReadyToInstall => "Update ready",
                UpdateManager.State.Installing => "Installing update",
                UpdateManager.State.InstallationFailed => "Update failed",

                _ => "Unknown update state",
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

        if (container.IsShowing)
        {
            return;
        }
        // For now we only have the update popup, so it is safe to ignore
        // any existing popup
        ShowNewUpdatePopupToContainer(container);
    }

    private static void ShowNewUpdatePopupToContainer(PopupDisplayContainer container)
    {
        var popup = new UpdatePopup();
        container.Popup = popup;
        popup.Opacity = 0;
        Dispatcher.UIThread.InvokeAsync(container.Show);
    }
}
