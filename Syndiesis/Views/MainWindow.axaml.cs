using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Syndiesis.Views;

public partial class MainWindow : Window
{
    private readonly SettingsView _settingsView = new();

    public MainWindow()
    {
        InitializeComponent();
        InitializeEvents();
    }

    private void InitializeEvents()
    {
        _settingsView.SettingsSaved += OnSettingsSaved;
        mainView.SettingsRequested += OnSettingsRequested;
    }

    private void OnSettingsRequested()
    {
        ShowSettings();
    }

    private void OnSettingsSaved()
    {
        ShowMainView();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        mainView.Reset();
        mainView.Focus();
    }

    public void ShowSettings()
    {
        _settingsView.LoadFromSettings();
        pageTransition.Content = _settingsView;
    }

    public void ShowMainView()
    {
        mainView.ApplyCurrentSettings();
        pageTransition.Content = mainView;
        mainView.Focus();
    }
}
