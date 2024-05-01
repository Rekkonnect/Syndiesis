using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using System;

namespace Syndiesis.Views;

public partial class SettingsView : UserControl
{
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
        saveButton.Click += OnSaveClicked;
    }

    public void LoadFromSettings()
    {
        var settings = AppSettings.Instance;
        showTriviaCheck.IsChecked = settings.CreationOptions.ShowTrivia;
        enableExpandAllButtonCheck.IsChecked = settings.EnableExpandingAllNodes;
        typingDelaySlider.Value = settings.UserInputDelay.TotalMilliseconds;
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
        settings.EnableExpandingAllNodes = enableExpandAllButtonCheck.IsChecked is true;
        settings.CreationOptions.ShowTrivia = showTriviaCheck.IsChecked is true;
        SettingsSaved?.Invoke();
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
