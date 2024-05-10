using Avalonia.Controls;
using Avalonia.Input.Platform;
using System.Threading.Tasks;

namespace Syndiesis.Utilities;

public static class ControlExtensions
{
    public static void InvalidateAll(this Control control)
    {
        control.InvalidateArrange();
        control.InvalidateMeasure();
        control.InvalidateVisual();
    }

    public static void RecursivelyInvalidateAll(this Control control)
    {
        control.InvalidateAll();
        var parent = control.Parent;

        if (parent is Control parentControl)
        {
            parentControl.InvalidateAll();
        }
    }

    public static IClipboard? Clipboard(this Control control)
    {
        return TopLevel.GetTopLevel(control)?.Clipboard;
    }

    public static async Task<string?> GetClipboardTextAsync(this Control control)
    {
        var clipboard = control.Clipboard();
        if (clipboard is null)
            return null;

        return await clipboard.GetTextAsync();
    }

    public static async Task SetClipboardTextAsync(this Control control, string? value)
    {
        var clipboard = control.Clipboard();
        if (clipboard is null)
            return;

        await clipboard.SetTextAsync(value);
    }
}
