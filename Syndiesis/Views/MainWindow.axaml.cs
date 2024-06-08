using Avalonia;
using Avalonia.Controls;
using Avalonia.Diagnostics;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Microsoft.CodeAnalysis;
using Syndiesis.Core;

namespace Syndiesis.Views;

public partial class MainWindow : Window
{
    private readonly MainView _mainView = new();
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
        SetCurrentTitle();

        pageTransition.SetMainContent(_mainView);
        pageTransition.SetSecondaryContent(_settingsView);
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

    private void SetCurrentTitle()
    {
        var currentLanguage = GetCurrentLanguageName();
        SetTitleForLanguage(currentLanguage);
    }

    private static string ProgramTitleForLanguage(string languageName)
    {
        return languageName switch
        {
            LanguageNames.CSharp => "Syndiesis",
            LanguageNames.VisualBasic => "SymVBiosis",
            _ => throw RoslynExceptions
                .ThrowInvalidLanguageArgument(languageName, nameof(languageName)),
        };
    }

    private void SetTitleForLanguage(string languageName)
    {
        var title = ProgramTitleForLanguage(languageName);
        SetTitle(title);
    }

    private void SetTitle(string programTitle)
    {
        var infoVersion = App.Current.AppInfo.InformationalVersion;
        Title = $"{programTitle} v{infoVersion.Version} [{infoVersion.CommitSha![..7]}]";
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
        if (languageName is not null)
        {
            SetThemeAndLogo(languageName);
        }
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
        var image = TitleBar.LogoImage;
        Icon = new WindowIcon(image);
        SetCurrentTitle();
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
