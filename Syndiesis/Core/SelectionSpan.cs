using Microsoft.CodeAnalysis.Text;

namespace Syndiesis.Core;

public class SelectionSpan
{
    public LinePosition SelectionStart { get; set; }
    public LinePosition SelectionEnd { get; set; }

    public bool HasSelection => !SelectionPositionSpan.IsEmpty();

    public bool IsBackwards => SelectionEnd < SelectionStart;

    public LinePositionSpan SelectionPositionSpan
    {
        get
        {
            return LineExtensions.FromBounds(SelectionStart, SelectionEnd);
        }
    }

    public void Clear()
    {
        SelectionStart = default;
        SelectionEnd = default;
    }

    public void SetBoth(LinePosition position)
    {
        SelectionStart = position;
        SelectionEnd = position;
    }

    public void SetBounds(LinePosition start, LinePosition end)
    {
        SelectionStart = start;
        SelectionEnd = end;
    }

    public void CoverUntil(LinePosition position)
    {
        if (IsBackwards)
        {
            if (position < SelectionEnd)
            {
                SelectionEnd = position;
            }
            if (position > SelectionStart)
            {
                SelectionStart = position;
            }
        }
        else
        {
            if (position < SelectionStart)
            {
                SelectionStart = position;
            }
            if (position > SelectionEnd)
            {
                SelectionEnd = position;
            }
        }
    }

    public void CoverRange(LinePositionSpan span)
    {
        CoverUntil(span.Start);
        CoverUntil(span.End);
    }
}
