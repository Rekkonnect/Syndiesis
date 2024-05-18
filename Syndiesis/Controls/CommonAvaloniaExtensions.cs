using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Reactive;
using Avalonia.Styling;
using System;
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

    public static void Subscribe<T>(this IObservable<T> observable, Action<T> action)
    {
        var observer = new AnonymousObserver<T>(action);
        observable.Subscribe(observer);
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

    public static void InvertMaximizedWindowState(this Window window)
    {
        window.WindowState = InvertMaximizedState(window.WindowState);
    }

    public static WindowState InvertMaximizedState(this WindowState windowState)
    {
        return windowState switch
        {
            WindowState.Normal => WindowState.Maximized,
            _ => WindowState.Normal,
        };
    }

    /// <summary>
    /// This is preferred over <see cref="Window.OffScreenMargin"/>. The property returns 7,
    /// but content still appears clipped. With manual testing, 7.5 is found to be the real value
    /// we should use.
    /// </summary>
    /// <param name="window"></param>
    /// <returns></returns>
    public static Thickness SpeculatedOffScreenMargin(this Window window)
    {
        if (OperatingSystem.IsWindows())
        {
            if (window.WindowState is WindowState.Maximized)
            {
                return new(7.5);
            }
        }

        return default;
    }
}

// Control
public static partial class CommonAvaloniaExtensions
{
    public static TAncestor? AncestorOf<TAncestor>(this Control control)
        where TAncestor : InputElement
    {
        var parent = control.Parent;
        while (parent is not null)
        {
            if (parent is TAncestor ancestor)
                return ancestor;

            parent = parent.Parent;
        }

        return null;
    }

    public static bool LoseFocus(this Control control)
    {
        var parent = control.Parent;
        while (parent is not null)
        {
            if (parent is InputElement parentControl)
            {
                var focused = parentControl.Focus();
                if (focused)
                    return true;
            }

            parent = parent.Parent;
        }

        return false;
    }

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
