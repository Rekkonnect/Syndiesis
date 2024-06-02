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
        InitializeHeader();
    }

    private void InitializeHeader()
    {
        var infoVersion = App.Current.AppInfo.InformationalVersion;
        Title = $"Syndiesis v{infoVersion.Version} [{infoVersion.CommitSha![..7]}]";
    }

    private void InitializeEvents()
    {
        _settingsView.SettingsSaved += OnSettingsSaved;
        _settingsView.SettingsReset += OnSettingsReset;
        _settingsView.SettingsCancelled += OnSettingsCancelled;
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

    private void OnSettingsCancelled()
    {
        TransitionIntoMainView();
    }

    private void OnSettingsReset()
    {
        ApplySettings();
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
        pageTransition.IsTransitionReversed = false;
        pageTransition.Content = _settingsView;
    }

    public void ShowMainView()
    {
        ApplySettings();
        TransitionIntoMainView();
    }

    private void ApplySettings()
    {
        mainView.ApplyCurrentSettings();
    }

    private void TransitionIntoMainView()
    {
        pageTransition.IsTransitionReversed = true;
        pageTransition.Content = mainView;
        mainView.Focus();
    }
}
