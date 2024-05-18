using Avalonia;
using Avalonia.Controls;
using Garyon.Extensions;
using System.Collections.Generic;

namespace Syndiesis.Controls.Inlines;

public class GroupedRunInlineTextBlock : TextBlock
{
    private GroupedRunInlineCollection? _groupedInlines;

    public GroupedRunInlineCollection? GroupedInlines
    {
        get => _groupedInlines;
        set
        {
            _groupedInlines = value;
            Inlines = _groupedInlines?.AsInlineCollection();
        }
    }

    public GroupedRunInline? HitTestGroupedRun(Point point)
    {
        if (_groupedInlines is null)
            return default;

        var hitTest = TextLayout.HitTestPoint(point);
        if (!hitTest.IsInside)
            return default;

        int index = hitTest.TextPosition;
        return GroupedRunForPosition(index);
    }

    public GroupedRunInline? GroupedRunForPosition(int index)
    {
        return GroupedRunForPositionCore(index, _groupedInlines, 0);
    }

    private GroupedRunInline? GroupedRunForPositionCore(
        int index,
        IReadOnlyList<object>? groupedInlines,
        int startIndex)
    {
        if (groupedInlines is null)
            return default;

        int currentIndex = startIndex;
        for (int i = 0; i < groupedInlines.Count; i++)
        {
            var current = RunOrGrouped.FromObject(groupedInlines[i]);
            var length = GroupedRunInline.GetTextLength(current);
            int endIndex = currentIndex + length;
            if (currentIndex <= index && index <= endIndex)
            {
                var grouped = current.Grouped;
                if (grouped is ComplexGroupedRunInline complex)
                {
                    var childrenInlines = complex.InlineObjects
                        .ToReadOnlyListOrExisting();
                    var result = GroupedRunForPositionCore(index, childrenInlines, currentIndex);
                    if (result is not null)
                        return result;
                }
                if (grouped is not null)
                    return grouped;
            }
            currentIndex += length;
        }

        return default;
    }

    public Rect? RunBounds(RunOrGrouped inline)
    {
        if (_groupedInlines is null)
            return null;

        return RunBounds(inline, _groupedInlines, 0);
    }

    private Rect? RunBounds(RunOrGrouped inline, IReadOnlyList<object?> container, int startIndex)
    {
        int currentIndex = startIndex;
        var soughtInline = inline.AvailableObject;
        foreach (var value in container)
        {
            var available = RunOrGrouped.FromObject(value);
            int length = GroupedRunInline.GetTextLength(available);
            int endIndex = currentIndex + length;
            if (soughtInline == value)
            {
                return HitTestTextRangeRect(currentIndex, length);
            }

            if (value is GroupedRunInline grouped)
            {
                var childRect = RunBounds(
                    inline,
                    grouped.InlineObjects.ToReadOnlyListOrExisting(),
                    currentIndex);

                if (childRect is not null)
                    return childRect;
            }

            currentIndex += length;
        }

        return null;
    }

    private Rect HitTestTextRangeRect(int start, int length)
    {
        var range = TextLayout.HitTestTextRange(start, length)
            .ToReadOnlyListOrExisting();

        if (range is [])
            return default;

        var rect = range[0];
        for (int i = 1; i < range.Count; i++)
        {
            var current = range[i];
            rect = rect.Union(current);
        }
        return rect;
    }
}
