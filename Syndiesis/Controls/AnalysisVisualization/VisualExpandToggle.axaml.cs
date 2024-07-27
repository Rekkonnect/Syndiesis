using Avalonia;
using Avalonia.Controls;

namespace Syndiesis.Controls.AnalysisVisualization;

public partial class VisualExpandToggle : UserControl
{
    public static readonly StyledProperty<bool> IsExpandingToggleProperty =
        AvaloniaProperty.Register<CodeEditor, bool>(nameof(IsExpandingToggle), defaultValue: false);

    public bool IsExpandingToggle
    {
        get => GetValue(IsExpandingToggleProperty);
        set
        {
            SetValue(IsExpandingToggleProperty, value);
            verticalLine.IsVisible = value;
        }
    }

    public VisualExpandToggle()
    {
        InitializeComponent();
    }

    public void Toggle()
    {
        IsExpandingToggle = !IsExpandingToggle;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        switch (change?.Property.Name)
        {
            case nameof(Foreground):
                horizontalLine.Fill = Foreground;
                verticalLine.Fill = Foreground;
                break;
        }
    }
}
