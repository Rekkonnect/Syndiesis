using Avalonia.Input;
using AvaloniaEdit.Rendering;
using Syndiesis.Utilities;

namespace Syndiesis.Controls;

public sealed class SyndiesisTextView : TextView
{
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        var modifiers = e.KeyModifiers.NormalizeByPlatform();
        if (modifiers.HasFlag(KeyModifiers.Control))
        {
            // We want Ctrl+wheel to change the font size,
            // so we prevent it from here
            return;
        }

        base.OnPointerWheelChanged(e);
    }
}
