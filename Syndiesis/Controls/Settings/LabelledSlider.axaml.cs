using Avalonia.Controls;

namespace Syndiesis.Controls.Settings;

public partial class LabelledSlider : UserControl
{
    public string ValueText
    {
        get => ValueTextBlock.Text!;
        set => ValueTextBlock.Text = value;
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
