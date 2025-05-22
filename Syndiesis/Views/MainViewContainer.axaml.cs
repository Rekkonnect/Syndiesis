using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Syndiesis.Core;

namespace Syndiesis.Views;

public partial class MainViewContainer : UserControl
{
    private readonly MainView _mainView = new();
    private readonly SettingsView _settingsView = new();

    public MainView MainView => _mainView;

    public MainViewContainer()
    {
        InitializeComponent();

        InitializeEvents();
        InitializeTransitions();
    }

    private void InitializeTransitions()
    {
        pageTransition.SetMainContent(_mainView);
        pageTransition.SetSecondaryContent(_settingsView);
    }

    private void InitializeEvents()
    {
        _settingsView.SettingsSaved += OnSettingsSaved;
        _settingsView.SettingsReset += OnSettingsReset;
        _settingsView.SettingsCancelled += OnSettingsCancelled;
        _mainView.SettingsRequested += OnSettingsRequested;
        _mainView.AnalysisPipelineHandler.AnalysisCompleted += OnAnalysisCompleted;
        TitleBar.LogoClicked += OnImageClicked;
    }

    private void OnAnalysisCompleted(AnalysisResult analysisResult)
    {
        Dispatcher.UIThread.InvokeAsync(UpdateTitleBar);
    }

    private void UpdateTitleBar()
    {
        var languageName = GetCurrentLanguageName();
        SetThemeAndLogo(languageName);
    }

    private string GetCurrentLanguageName()
    {
        return _mainView.ViewModel.HybridCompilationSource.CurrentLanguageName;
    }

    private void OnImageClicked(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        var properties = point.Properties;
        if (properties.IsLeftButtonPressed && e.KeyModifiers is KeyModifiers.None)
        {
            var toggled = _mainView.ToggleLanguage();
            SetThemeAndLogo(toggled);
        }
    }

    private void SetThemeAndLogo(string languageName)
    {
        TitleBar.SetThemeForLanguage(languageName);
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
        _mainView.Reset();
        _mainView.Focus();
    }

    public void ShowSettings()
    {
        _settingsView.LoadFromSettings();
        pageTransition.TransitionToSecondary();
    }

    public void ShowMainView()
    {
        ApplySettings();
        TransitionIntoMainView();
    }

    private void ApplySettings()
    {
        _mainView.ApplyCurrentSettings();
    }

    private void TransitionIntoMainView()
    {
        pageTransition.TransitionToMain();
    }
}
