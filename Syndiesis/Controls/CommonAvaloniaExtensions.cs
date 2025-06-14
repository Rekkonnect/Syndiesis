﻿using Avalonia.Collections;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Reactive;
using Avalonia.Styling;

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

    public static void ClearSetValue<T>(this AvaloniaList<T> source, T value)
    {
        source.Clear();
        source.Add(value);
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

    public static T? NearestAncestorOfType<T>(this StyledElement element)
        where T : StyledElement
    {
        StyledElement? current = element;
        while (true)
        {
            if (current is null)
                return null;

            var parent = current.Parent;
            if (parent is T ancestor)
                return ancestor;

            current = parent;
        }
    }
}

// Components
public static partial class CommonAvaloniaExtensions
{
    public static Rect WithZeroOffset(this Rect rect)
    {
        return new(0, 0, rect.Width, rect.Height);
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

    public static void SetHorizontalOffset(this ScrollViewer scrollViewer, double value)
    {
        var offset = scrollViewer.Offset;
        scrollViewer.Offset = new(value, offset.Y);
    }

    public static void SetVerticalOffset(this ScrollViewer scrollViewer, double value)
    {
        var offset = scrollViewer.Offset;
        scrollViewer.Offset = new(offset.X, value);
    }

    public static Vector Add(this Vector vector, Size size)
    {
        return vector + new Vector(size.Width, size.Height);
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

    public static void AddIfNotContained(this ControlList controls, Control control)
    {
        if (!controls.Contains(control))
        {
            controls.Add(control);
        }
    }

    public static bool ContainsPointer(this Control control, PointerEventArgs pointer)
    {
        var position = pointer.GetPosition(control);
        return control.Bounds.Contains(position);
    }
}

// Dispatcher
public static partial class CommonAvaloniaExtensions
{
    public static void ExecuteOrDispatch(this Dispatcher dispatcher, Action action)
    {
        bool access = dispatcher.CheckAccess();
        if (access)
        {
            action();
        }
        else
        {
            dispatcher.Invoke(action);
        }
    }

    public static void ExecuteOrDispatchUI(Action action)
    {
        var dispatcher = Dispatcher.UIThread;
        dispatcher.ExecuteOrDispatch(action);
    }

    public static T ExecuteOrDispatch<T>(this Dispatcher dispatcher, Func<T> func)
    {
        bool access = dispatcher.CheckAccess();
        if (access)
        {
            return func();
        }
        else
        {
            return dispatcher.Invoke(func);
        }
    }

    public static T ExecuteOrDispatchUI<T>(Func<T> func)
    {
        var dispatcher = Dispatcher.UIThread;
        return dispatcher.ExecuteOrDispatch(func);
    }
}
