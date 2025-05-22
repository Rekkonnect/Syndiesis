using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Syndiesis.Controls;
using Syndiesis.Controls.Toast;
using Syndiesis.Utilities;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Syndiesis.Views;

public partial class SettingsView : UserControl
{
    private int TypingDelayMilliseconds => typingDelaySlider.ValueSlider.Value.RoundInt32();
    private int HoverInfoDelayMilliseconds => hoverInfoDelaySlider.ValueSlider.Value.RoundInt32();
    private int IndentationWidth => indentationWidthSlider.ValueSlider.Value.RoundInt32();
    private int RecursiveExpansionDepth => recursiveExpansionDepthSlider.ValueSlider.Value.RoundInt32();

    private TimeSpan TypingDelay => TimeSpan.FromMilliseconds(TypingDelayMilliseconds);
    private TimeSpan HoverInfoDelay => TimeSpan.FromMilliseconds(HoverInfoDelayMilliseconds);

    public event Action? SettingsSaved;
    public event Action? SettingsReset;
    public event Action? SettingsCancelled;

    public SettingsView()
    {
        InitializeComponent();
        InitializeEvents();
        InitializeSliders();
    }

    private void InitializeEvents()
    {
        typingDelaySlider.ValueSlider.ValueChanged += OnDelaySliderValueChanged;
        hoverInfoDelaySlider.ValueSlider.ValueChanged += OnHoverInfoDelayValueChanged;
        indentationWidthSlider.ValueSlider.ValueChanged += OnIndentationSliderValueChanged;
        recursiveExpansionDepthSlider.ValueSlider.ValueChanged += OnRecursiveExpansionDepthValueChanged;
        saveButton.Click += OnSaveClicked;
        cancelButton.Click += OnCancelClicked;
        resetButton.Click += OnResetClicked;
        openLogsButton.AttachAsyncClick(OpenLogs);
        viewSettingsFileButton.AttachAsyncClick(ViewSettingsFile);
    }

    public void LoadFromSettings()
    {
        var settings = AppSettings.Instance;
        showTriviaCheck.IsChecked = settings.NodeLineOptions.ShowTrivia;
        showWhitespaceGlyphsCheck.IsChecked = settings.ShowWhitespaceGlyphs;
        wordWrapCheck.IsChecked = settings.WordWrap;
        enableColorizationCheck.IsChecked = settings.EnableColorization;
        enableSemanticColorizationCheck.IsChecked = settings.EnableSemanticColorization;
        automaticallyDetectLanguageCheck.IsChecked = settings.AutomaticallyDetectLanguage;
        typingDelaySlider.ValueSlider.Value = settings.UserInputDelay.TotalMilliseconds;
        hoverInfoDelaySlider.ValueSlider.Value = settings.HoverInfoDelay.TotalMilliseconds;
        indentationWidthSlider.ValueSlider.Value = settings.IndentationOptions.IndentationWidth;
        recursiveExpansionDepthSlider.ValueSlider.Value = settings.RecursiveExpansionDepth;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        LoadFromSettings();
        base.OnLoaded(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        var modifiers = e.KeyModifiers.NormalizeByPlatform();

        switch (e.Key)
        {
            case Key.Escape:
                CancelSettings();
                break;

            case Key.R:
                if (modifiers is (KeyModifiers.Control | KeyModifiers.Shift))
                {
                    ResetSettings();
                    e.Handled = true;
                }

                break;

            case Key.S:
                if (modifiers is KeyModifiers.Control)
                {
                    SaveSettings();
                    e.Handled = true;
                }

                break;
        }
    }

    private void OnSaveClicked(object? sender, RoutedEventArgs e)
    {
        SaveSettings();
    }

    private void OnResetClicked(object? sender, RoutedEventArgs e)
    {
        ResetSettings();
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e)
    {
        CancelSettings();
    }

    private void OpenLogs()
    {
        var currentDirectory = CurrentExecutingDirectory();
        var fullPath = Path.Combine(currentDirectory.FullName, "logs");
        ProcessUtilities.ShowDirectoryInFileViewer(fullPath)
            .AwaitProcessInitialized();
    }

    private void ViewSettingsFile()
    {
        var currentDirectory = CurrentExecutingDirectory();
        var fullPath = Path.Combine(currentDirectory.FullName, AppSettings.DefaultPath);
        ProcessUtilities.ShowFileInFileViewer(fullPath)
            .AwaitProcessInitialized();
    }

    private static DirectoryInfo CurrentExecutingDirectory()
    {
        var currentPath = Assembly.GetExecutingAssembly().Location;
        return Directory.GetParent(currentPath)!;
    }

    private void CancelSettings()
    {
        SettingsCancelled?.Invoke();
    }

    private void ResetSettings()
    {
        Dispatcher.UIThread.Invoke(ResetSettingsAsync);
    }

    private async Task ResetSettingsAsync()
    {
        bool success = await AppSettings.TryLoad();
        var notificationContainer = ToastNotificationContainer.GetFromOuterMainViewContainer(this);
        if (success)
        {
            LoadFromSettings();
            _ = CommonToastNotifications.ShowClassicMain(
                notificationContainer,
                "Reverted settings to current file state",
                TimeSpan.FromSeconds(2));
            SettingsReset?.Invoke();
        }
        else
        {
            _ = CommonToastNotifications.ShowClassicFailure(
                notificationContainer,
                "Failed to reset settings. Please check the logs for details.",
                TimeSpan.FromSeconds(4));
        }
    }

    private void SaveSettings()
    {
        Dispatcher.UIThread.InvokeAsync(SaveSettingsAsync);
    }

    private async Task SaveSettingsAsync()
    {
        var settings = AppSettings.Instance;
        SetSettingsValues(settings);

        var path = AppSettings.DefaultPath;
        bool success = await AppSettings.TrySave(path);
        var notificationContainer = ToastNotificationContainer.GetFromOuterMainViewContainer(this);
        if (success)
        {
            _ = CommonToastNotifications.ShowClassicMain(
                notificationContainer,
                "Settings saved successfully",
                TimeSpan.FromSeconds(2));
            SettingsSaved?.Invoke();
        }
        else
        {
            var fileInfo = new FileInfo(path);

            _ = CommonToastNotifications.ShowClassicFailure(
                notificationContainer,
                $"""
                 Failed to save settings to path:
                 '{fileInfo.FullName}'
                 Please check the logs for details.
                 """,
                TimeSpan.FromSeconds(4));
        }
    }

    private void SetSettingsValues(AppSettings settings)
    {
        settings.UserInputDelay = TypingDelay;
        settings.HoverInfoDelay = HoverInfoDelay;
        settings.RecursiveExpansionDepth = RecursiveExpansionDepth;
        settings.IndentationOptions.IndentationWidth = IndentationWidth;
        settings.NodeLineOptions.ShowTrivia = showTriviaCheck.IsChecked is true;
        settings.ShowWhitespaceGlyphs = showWhitespaceGlyphsCheck.IsChecked is true;
        settings.EnableColorization = enableColorizationCheck.IsChecked is true;
        settings.EnableSemanticColorization = enableSemanticColorizationCheck.IsChecked is true;
        settings.WordWrap = wordWrapCheck.IsChecked is true;
        settings.AutomaticallyDetectLanguage = automaticallyDetectLanguageCheck.IsChecked is true;
    }

    private void OnIndentationSliderValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        UpdateIndentationValue();
    }

    private void UpdateIndentationValue()
    {
        indentationWidthSlider.ValueText = $"{IndentationWidth}x space";
    }

    private void OnDelaySliderValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        UpdateTypingDelayValue();
    }

    private void UpdateTypingDelayValue()
    {
        typingDelaySlider.ValueText = $"{TypingDelayMilliseconds} ms";
    }

    private void OnHoverInfoDelayValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        UpdateHoverInfoDelayValue();
    }

    private void UpdateHoverInfoDelayValue()
    {
        hoverInfoDelaySlider.ValueText = $"{HoverInfoDelayMilliseconds} ms";
    }

    private void OnRecursiveExpansionDepthValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        UpdateRecursiveExpansionDepthValue();
    }

    private void UpdateRecursiveExpansionDepthValue()
    {
        recursiveExpansionDepthSlider.ValueText = $"{RecursiveExpansionDepth} levels";
    }

    private void InitializeSliders()
    {
        {
            var valueSlider = typingDelaySlider.ValueSlider;
            valueSlider.Minimum = 50;
            valueSlider.Maximum = 1000;
            valueSlider.Value = 600;
            valueSlider.SmallChange = 50;
            valueSlider.LargeChange = 100;
        }

        {
            var valueSlider = hoverInfoDelaySlider.ValueSlider;
            valueSlider.Minimum = 100;
            valueSlider.Maximum = 1000;
            valueSlider.Value = 400;
            valueSlider.SmallChange = 50;
            valueSlider.LargeChange = 100;
        }

        {
            var valueSlider = indentationWidthSlider.ValueSlider;
            valueSlider.Minimum = 1;
            valueSlider.Maximum = 12;
            valueSlider.Value = 4;
            valueSlider.SmallChange = 1;
            valueSlider.LargeChange = 2;
            valueSlider.TickFrequency = 1;
            valueSlider.IsSnapToTickEnabled = true;
        }

        {
            var valueSlider = recursiveExpansionDepthSlider.ValueSlider;
            valueSlider.Minimum = 3;
            valueSlider.Maximum = 10;
            valueSlider.Value = 4;
            valueSlider.SmallChange = 1;
            valueSlider.LargeChange = 2;
            valueSlider.TickFrequency = 1;
            valueSlider.IsSnapToTickEnabled = true;
        }
    }
}
