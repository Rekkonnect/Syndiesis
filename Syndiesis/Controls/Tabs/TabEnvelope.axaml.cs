using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Layout;
using Syndiesis.Utilities;
using System;

namespace Syndiesis.Controls.Tabs;

public partial class TabEnvelope : UserControl
{
    private bool _isSelected = false;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            UpdateLowerBorder();
        }
    }

    public string? Text
    {
        get => text.Text;
        set => text.Text = value;
    }

    public int Index { get; set; }

    public object? TagValue { get; set; }

    public TabEnvelope()
    {
        InitializeComponent();
        InitializeTransitions();
    }

    private void InitializeTransitions()
    {
        // NRE is thrown when setting transitions from XAML;
        // the Transitions property is null and not initialized
        lowerBorder.Transitions =
        [
            new ThicknessTransition
            {
                Property = Layoutable.MarginProperty,
                Duration = TimeSpan.FromMilliseconds(350),
                Easing = Singleton<ExponentialEaseOut>.Instance,
            }
        ];
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateLowerBorder();
    }

    private void UpdateLowerBorder()
    {
        double top = GetRequiredLowerBorderMarginTop();
        lowerBorder.Margin = lowerBorder.Margin.WithTop(top);
    }

    private double GetRequiredLowerBorderMarginTop()
    {
        return _isSelected ? Bounds.Height : 2;
    }
}
