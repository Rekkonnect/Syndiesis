using Avalonia;
using Avalonia.Controls;
using Avalonia.Diagnostics;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;

namespace Syndiesis.Views;

public partial class MainWindow : Window
{
    private readonly SettingsView _settingsView = new();

    public MainWindow()
    {
        // Truly a shame
#if DEBUG
        InitializeComponent(attachDevTools: false);
#else
        InitializeComponent();
#endif
        AttachDevTools();
        InitializeEvents();
        InitializeHeader();
    }

    private void AttachDevTools()
    {
#if DEBUG
        var options = new DevToolsOptions
        {
            Gesture = new(Key.F10),
        };
        this.AttachDevTools(options);
#endif
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
        mainView.codeEditor.AnalysisCompleted += OnAnalysisCompleted;
        TitleBar.LogoClicked += OnImageClicked;
    }

    private void OnAnalysisCompleted()
    {
        Dispatcher.UIThread.InvokeAsync(UpdateTitleBar);
    }

    private void UpdateTitleBar()
    {
        var languageName = mainView.ViewModel.HybridCompilationSource.CurrentLanguageName;
        if (languageName is not null)
        {
            TitleBar.SetThemeForLanguage(languageName);
        }
    }

    private void OnImageClicked(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        var properties = point.Properties;
        if (properties.IsLeftButtonPressed && e.KeyModifiers is KeyModifiers.None)
        {
            var toggled = mainView.ToggleLanguage();
            TitleBar.SetThemeForLanguage(toggled);
        }
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
