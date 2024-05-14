using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Styling;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Syndiesis.Controls;

// Not the wisest naming choice
using ControlList = Avalonia.Controls.Controls;

// General
public static partial class CommonAvaloniaExtensions
{
    public static void ApplySetter(this Setter setter, AvaloniaObject control)
    {
        control.SetValue(setter.Property!, setter.Value);
    }

    public static void ClearSetValues<T>(this AvaloniaList<T> source, IReadOnlyList<T> values)
    {
        source.Clear();
        source.AddRange(values);
    }
}

// Components
public static partial class CommonAvaloniaExtensions
{
    public static Thickness WithLeft(this Thickness thickness, double left)
    {
        return new(left, thickness.Top, thickness.Right, thickness.Bottom);
    }
    public static Thickness WithRight(this Thickness thickness, double right)
    {
        return new(thickness.Left, thickness.Top, right, thickness.Bottom);
    }
    public static Thickness WithTop(this Thickness thickness, double top)
    {
        return new(thickness.Left, top, thickness.Right, thickness.Bottom);
    }
    public static Thickness WithBottom(this Thickness thickness, double bottom)
    {
        return new(thickness.Left, thickness.Top, thickness.Right, bottom);
    }

    public static async Task<bool> HasFormatAsync(this IClipboard? clipboard, string format)
    {
        if (clipboard is null)
            return false;

        var formats = await clipboard.GetFormatsAsync();
        return formats.Contains(format);
    }
}

// Control
public static partial class CommonAvaloniaExtensions
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

    public static async Task SetClipboardDataAsync(this Control control, IDataObject value)
    {
        var clipboard = control.Clipboard();
        if (clipboard is null)
            return;

        await clipboard.SetDataObjectAsync(value);
    }

    public static async Task<bool> HasSingleLineClipboardText(this Control control)
    {
        var clipboard = control.Clipboard();
        if (clipboard is null)
            return false;

        return await clipboard.HasFormatAsync(CodeEditorDataObject.Formats.CodeEditor);
    }

    public static void AddIfNotContained(this ControlList controls, Control control)
    {
        if (!controls.Contains(control))
        {
            controls.Add(control);
        }
    }
}
