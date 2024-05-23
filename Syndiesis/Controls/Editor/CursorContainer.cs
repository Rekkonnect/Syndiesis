using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System;

namespace Syndiesis.Controls;

using AvaCursor = Avalonia.Input.Cursor;

public class CursorContainer : AvaloniaObject
{
    public static readonly AttachedProperty<AvaCursor> CursorProperty =
        AvaloniaProperty.RegisterAttached<InputElement, AvaCursor>(
            "Cursor",
            typeof(CursorContainer),
            inherits: true);

    static CursorContainer()
    {
        CursorProperty.Changed.AddClassHandler<InputElement>(HandleAttachedCursorChanged);
    }

    private static void HandleAttachedCursorChanged(
        InputElement element, AvaloniaPropertyChangedEventArgs args)
    {
        var cursor = args.NewValue as AvaCursor;
        if (cursor is null)
            return;
    }

    public static AvaCursor GetCursor(InputElement element)
    {
        return element.GetValue(CursorProperty);
    }

    public static void SetCursor(InputElement element, AvaCursor value)
    {
        element.SetValue(CursorProperty, value);

        element.PointerEntered += HandlePointerEntered;
        element.PointerExited += HandlePointerExited;
        element.PointerPressed += HandlePointerPressed;
        element.DoubleTapped += HandleTapped;
        element.Tapped += HandleTapped;

        void HandlePointerEntered(object? sender, PointerEventArgs e)
        {
            if (sender != element)
                return;

            var top = TopLevel.GetTopLevel(element)!;
            top.Cursor = value;
        }
        void HandlePointerExited(object? sender, PointerEventArgs e)
        {
            HandleAbstractEvent(e.GetPosition(element));
        }
        void HandlePointerPressed(object? sender, PointerEventArgs e)
        {
            HandleAbstractEvent(e.GetPosition(element));
        }
        void HandleTapped(object? sender, TappedEventArgs e)
        {
            HandleAbstractEvent(e.GetPosition(element));
        }

        void HandleAbstractEvent(Point position)
        {
            var top = TopLevel.GetTopLevel(element)!;

            if (element.Bounds.Contains(position))
            {
                top.Cursor = value;
                return;
            }

            top.Cursor = default;
        }
    }
}
