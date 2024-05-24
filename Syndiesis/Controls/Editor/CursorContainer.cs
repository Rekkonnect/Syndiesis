using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using System.Collections.Generic;

namespace Syndiesis.Controls;

using AvaCursor = Avalonia.Input.Cursor;

public class CursorContainer : AvaloniaObject
{
    public static readonly AttachedProperty<AvaCursor> CursorProperty =
        AvaloniaProperty.RegisterAttached<InputElement, AvaCursor>(
            "Cursor",
            typeof(CursorContainer),
            inherits: true);

    private static readonly VisualRootMappingDictionary _mappingDictionary = new();

    private readonly InputElement _root;
    private readonly List<ElementCursorMapping> _mappings = new();

    private PointerEventArgs? _lastPointerEventArgs;

    public CursorContainer(InputElement root)
    {
        _root = root;
        InitializeEvents();
    }

    private void InitializeEvents()
    {
        _root.PointerMoved += HandlePointerMoved;
    }

    private void HandlePointerMoved(object? sender, PointerEventArgs e)
    {
        _lastPointerEventArgs = e;
        EvaluateCursor();
    }

    private void EvaluateCursor()
    {
        if (_lastPointerEventArgs is null)
            return;

        int appliedIndex = int.MinValue;
        AvaCursor? appliedCursor = null;

        foreach (var mapping in _mappings)
        {
            var (element, cursor) = mapping;
            var root = element.GetVisualRoot();
            if (root is null)
                continue;

            if (element.ZIndex <= appliedIndex)
                continue;

            var position = _lastPointerEventArgs.GetPosition(element);
            var zeroedBounds = element.Bounds.WithZeroOffset();
            if (zeroedBounds.Contains(position))
            {
                appliedCursor = cursor;
                appliedIndex = element.ZIndex;
            }
        }

        _root.Cursor = appliedCursor;
    }

    public void Add(ElementCursorMapping mapping)
    {
        _mappings.Add(mapping);
        var visual = mapping.Visual;
        visual.AttachedToVisualTree += HandleAttached;
        visual.DetachedFromLogicalTree += HandleDetached;
    }

    public void Add(Visual visual, AvaCursor cursor)
    {
        Add(new(visual, cursor));
    }

    private void Remove(Visual target)
    {
        int index = _mappings.FindIndex(s => s.Visual == target);
        if (index < 0)
            return;

        target.AttachedToVisualTree -= HandleAttached;
        target.DetachedFromLogicalTree -= HandleDetached;
        _mappings.RemoveAt(index);
    }

    public static AvaCursor GetCursor(InputElement element)
    {
        return element.GetValue(CursorProperty);
    }

    private void HandleDetached(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        EvaluateCursor();
    }

    private void HandleAttached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        EvaluateCursor();
    }

    public static void SetCursor(InputElement element, AvaCursor value)
    {
        element.SetValue(CursorProperty, value);

        element.AttachedToVisualTree += HandleElementAttached;
    }

    private static void HandleElementAttached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        var root = e.Root as InputElement;
        var element = sender as Control;
        _mappingDictionary.Add(root!, element!, GetCursor(element!));
    }

    public sealed record ElementCursorMapping(Visual Visual, AvaCursor? Cursor);

    private sealed class VisualRootMappingDictionary
    {
        private readonly Dictionary<InputElement, CursorContainer> _dictionary = new();

        public CursorContainer? ContainerForRoot(InputElement root)
        {
            if (_dictionary.TryGetValue(root, out var value))
                return value;

            return null;
        }

        public void Add(InputElement root, Visual target, AvaCursor? cursor)
        {
            Add(root, new(target, cursor));
        }

        public void Add(InputElement root, ElementCursorMapping mapping)
        {
            var contained = _dictionary.TryGetValue(root, out var container);
            if (!contained)
            {
                container = new(root);
                _dictionary.Add(root, container);
            }

            container!.Add(mapping);
        }

        public void Remove(InputElement root, Visual target)
        {
            var contained = _dictionary.TryGetValue(root, out var container);
            if (!contained)
                return;

            container!.Remove(target);
        }
    }
}
