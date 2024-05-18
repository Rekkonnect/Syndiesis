using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Syndiesis.Controls.Toast;
using Syndiesis.Utilities;
using System;
using System.IO;

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
        SetSettingsValues(settings);
        var path = AppSettings.DefaultPath;
        bool success = AppSettings.TrySave(path);
        var notificationContainer = ToastNotificationContainer.GetFromMainWindowTopLevel(this);
        if (success)
        {
            if (notificationContainer is not null)
            {
                var popup = new ToastNotificationPopup();
                popup.defaultTextBlock.Text = "Settings saved successfully";
                var animation = new BlurOpenDropCloseToastAnimation(TimeSpan.FromSeconds(2));
                _ = notificationContainer.Show(popup, animation);
            }
        }
        else
        {
            if (notificationContainer is not null)
            {
                var fileInfo = new FileInfo(path);
                var popup = new ToastNotificationPopup();
                popup.BackgroundFill = Color.FromUInt32(0xFF660030);
                popup.defaultTextBlock.Text = $"""
                    Failed to save settings to path:
                    '{fileInfo.FullName}'
                    Please check the logs for details.
                    """;
                var animation = new BlurOpenDropCloseToastAnimation(TimeSpan.FromSeconds(4));
                _ = notificationContainer.Show(popup, animation);
            }
        }
        SettingsSaved?.Invoke();
    }

    private void SetSettingsValues(AppSettings settings)
    {
        settings.UserInputDelay = TypingDelay;
        settings.RecursiveExpansionDepth = RecursiveExpansionDepth;
        settings.IndentationOptions.IndentationWidth = IndentationWidth;
        settings.EnableExpandingAllNodes = enableExpandAllButtonCheck.IsChecked is true;
        settings.NodeLineOptions.ShowTrivia = showTriviaCheck.IsChecked is true;
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
