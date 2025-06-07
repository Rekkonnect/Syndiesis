using Avalonia.Input;
using AvaloniaEdit.Editing;
using Syndiesis.Utilities;

namespace Syndiesis.Controls;

public sealed class SyndiesisTextArea : TextArea
{
    public const double MinFontSize = 4;
    public const double MaxFontSize = 60;

    public new SyndiesisTextView TextView => (SyndiesisTextView)base.TextView;

    public event Action? FontSizeChanged;

    public SyndiesisTextArea()
        : base(new SyndiesisTextView())
    {
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        var modifiers = e.KeyModifiers.NormalizeByPlatform();
        if (modifiers.HasFlag(KeyModifiers.Control))
        {
            ChangeTextSize(e);
            e.Handled = true;
            return;
        }

        base.OnPointerWheelChanged(e);
    }

    private void ChangeTextSize(PointerWheelEventArgs e)
    {
        const double zoomFactor = 0.05;
        const double baseZoomIncreaseMultiplier = 1 + zoomFactor;
        const double baseZoomDecreaseMultiplier = 1 - zoomFactor;

        double verticalSteps = e.Delta.Y;
        bool isDecreasingZoom = verticalSteps < 0;
        double multiplier = isDecreasingZoom
            ? baseZoomDecreaseMultiplier
            : baseZoomIncreaseMultiplier
            ;

        double power = Math.Abs(verticalSteps);
        var nextFontSizeMultiplier = Math.Pow(multiplier, power);
        var previousFontSize = FontSize;
        var nextFontSize = previousFontSize * nextFontSizeMultiplier;
        nextFontSize = Math.Clamp(nextFontSize, MinFontSize, MaxFontSize);
        if (previousFontSize != nextFontSize)
        {
            FontSize = nextFontSize;
            FontSizeChanged?.Invoke();
        }
    }
}
