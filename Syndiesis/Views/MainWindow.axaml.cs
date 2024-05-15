using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

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
        Title = $"Syndiesis v{infoVersion.Version} [{infoVersion.CommitSha![^7..]}]";
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
        pageTransition.IsTransitionReversed = false;
        pageTransition.Content = _settingsView;
    }

    public void ShowMainView()
    {
        mainView.ApplyCurrentSettings();
        pageTransition.IsTransitionReversed = true;
        pageTransition.Content = mainView;
        mainView.Focus();
    }
}
