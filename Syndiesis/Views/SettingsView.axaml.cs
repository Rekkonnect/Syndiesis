using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using System;

namespace Syndiesis.Views;

public partial class SettingsView : UserControl
{
    private int ExtraBufferLines => (int)extraBufferLinesSlider.Value;
    private int TypingDelayMilliseconds => (int)typingDelaySlider.Value;

    private TimeSpan TypingDelay => TimeSpan.FromMilliseconds(TypingDelayMilliseconds);

    public event Action? SettingsSaved;

    public SettingsView()
    {
        InitializeComponent();
        InitializeEvents();
    }

    private void InitializeEvents()
    {
        typingDelaySlider.ValueChanged += OnDelaySliderValueChanged;
        extraBufferLinesSlider.ValueChanged += OnExtraBufferLinesSiderValueChanged;
        saveButton.Click += OnSaveClicked;
    }

    public void LoadFromSettings()
    {
        var settings = AppSettings.Instance;
        showTriviaCheck.IsChecked = settings.CreationOptions.ShowTrivia;
        typingDelaySlider.Value = settings.UserInputDelay.TotalMilliseconds;
        extraBufferLinesSlider.Value = settings.ExtraBufferLines;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        LoadFromSettings();
        base.OnLoaded(e);
    }

    private void OnSaveClicked(object? sender, RoutedEventArgs e)
    {
        SaveSettings();
    }

    private void SaveSettings()
    {
        var settings = AppSettings.Instance;
        settings.UserInputDelay = TypingDelay;
        settings.ExtraBufferLines = ExtraBufferLines;
        settings.CreationOptions.ShowTrivia = showTriviaCheck.IsChecked is true;
        SettingsSaved?.Invoke();
    }

    private void OnExtraBufferLinesSiderValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        UpdateBufferLinesSliderValueDisplay();
    }

    private void UpdateBufferLinesSliderValueDisplay()
    {
        textEditorBufferValueDisplay.Text = ExtraBufferLines.ToString();
    }

    private void OnDelaySliderValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        UpdateSliderValueDisplay();
    }

    private void UpdateSliderValueDisplay()
    {
        sliderValueDisplay.Text = TypingDelayMilliseconds.ToString();
    }
}
