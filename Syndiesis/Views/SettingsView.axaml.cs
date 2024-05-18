using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Syndiesis.Utilities;
using System;

namespace Syndiesis.Views;

public partial class SettingsView : UserControl
{
    private int TypingDelayMilliseconds => typingDelaySlider.ValueSlider.Value.RoundInt32();
    private int IndentationWidth => indentationWidthSlider.ValueSlider.Value.RoundInt32();
    private int RecursiveExpansionDepth => recursiveExpansionDepthSlider.ValueSlider.Value.RoundInt32();

    private TimeSpan TypingDelay => TimeSpan.FromMilliseconds(TypingDelayMilliseconds);

    public event Action? SettingsSaved;

    public SettingsView()
    {
        InitializeComponent();
        InitializeEvents();
        InitializeSliders();
    }

    private void InitializeEvents()
    {
        typingDelaySlider.ValueSlider.ValueChanged += OnDelaySliderValueChanged;
        indentationWidthSlider.ValueSlider.ValueChanged += OnIndentationSliderValueChanged;
        recursiveExpansionDepthSlider.ValueSlider.ValueChanged += OnRecursiveExpansionDepthValueChanged;
        saveButton.Click += OnSaveClicked;
    }

    public void LoadFromSettings()
    {
        var settings = AppSettings.Instance;
        showTriviaCheck.IsChecked = settings.NodeLineOptions.ShowTrivia;
        enableExpandAllButtonCheck.IsChecked = settings.EnableExpandingAllNodes;
        typingDelaySlider.ValueSlider.Value = settings.UserInputDelay.TotalMilliseconds;
        indentationWidthSlider.ValueSlider.Value = settings.IndentationOptions.IndentationWidth;
        recursiveExpansionDepthSlider.ValueSlider.Value = settings.RecursiveExpansionDepth;
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
        settings.IndentationOptions.IndentationWidth = IndentationWidth;
        settings.EnableExpandingAllNodes = enableExpandAllButtonCheck.IsChecked is true;
        settings.NodeLineOptions.ShowTrivia = showTriviaCheck.IsChecked is true;
        AppSettings.TrySave();
        SettingsSaved?.Invoke();
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
            var valueSlider = indentationWidthSlider.ValueSlider;
            valueSlider.Minimum = 1;
            valueSlider.Maximum = 12;
            valueSlider.Value = 4;
            valueSlider.SmallChange = 1;
            valueSlider.LargeChange = 2;
        }

        {
            var valueSlider = recursiveExpansionDepthSlider.ValueSlider;
            valueSlider.Minimum = 3;
            valueSlider.Maximum = 10;
            valueSlider.Value = 4;
            valueSlider.SmallChange = 1;
            valueSlider.LargeChange = 2;
        }
    }
}
