using Avalonia;
using Avalonia.Controls;

namespace Syndiesis.Controls.Settings;

public partial class LabelledSlider : UserControl
{
    public static readonly DirectProperty<LabelledSlider, string> ValueTextProperty =
        AvaloniaProperty.RegisterDirect<LabelledSlider, string>(
            nameof(ValueText),
            o => o.ValueText,
            (o, v) => o.ValueText = v);

    public string ValueText
    {
        get => ValueTextBlock.Text!;
        set
        {
            var previous = ValueText;
            SetAndRaise(ValueTextProperty, ref previous, value);
            ValueTextBlock.Text = value;
        }
    }

    public string NameText
    {
        get => NameTextBlock.Text!;
        set => NameTextBlock.Text = value;
    }

    public Slider ValueSlider => ValueSliderField;

    public LabelledSlider()
    {
        InitializeComponent();
    }
}
