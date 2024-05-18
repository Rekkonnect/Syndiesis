using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;

namespace Syndiesis.Controls.Toast;

public partial class ToastProgressBar : UserControl
{
    public static readonly StyledProperty<double> MinValueProperty =
        AvaloniaProperty.Register<ToastProgressBar, double>(nameof(MinValue));

    public double MinValue
    {
        get => GetValue(MinValueProperty);
        set
        {
            SetValue(MinValueProperty, value);
        }
    }

    public static readonly StyledProperty<double> MaxValueProperty =
        AvaloniaProperty.Register<ToastProgressBar, double>(nameof(MaxValue));

    public double MaxValue
    {
        get => GetValue(MaxValueProperty);
        set
        {
            SetValue(MaxValueProperty, value);
        }
    }

    public static readonly StyledProperty<double> CurrentValueProperty =
        AvaloniaProperty.Register<ToastProgressBar, double>(nameof(CurrentValue));

    public double CurrentValue
    {
        get => GetValue(CurrentValueProperty);
        set
        {
            SetValue(CurrentValueProperty, value);
        }
    }

    public double ValueRange => MaxValue - MinValue;
    public double Progress => (CurrentValue - MinValue) / ValueRange;

    public IBrush? ProgressBarBrush
    {
        get => progressRectangle.Fill;
        set => progressRectangle.Fill = value;
    }

    public ToastProgressBar()
    {
        InitializeComponent();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        UpdateRectangleProgress();
        return base.MeasureOverride(availableSize);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        switch (change.Property.Name)
        {
            case nameof(MinValue):
            case nameof(MaxValue):
            case nameof(CurrentValue):
                UpdateRectangleProgress();
                break;
        }
    }

    private void UpdateRectangleProgress()
    {
        var progress = Math.Clamp(Progress, 0, 1);
        progressRectangle.Width = progress * progressBorder.Bounds.Width;
    }
}
