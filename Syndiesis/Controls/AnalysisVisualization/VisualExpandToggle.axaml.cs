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
}
