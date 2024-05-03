using Microsoft.CodeAnalysis.Text;
using Syndiesis.Utilities;

namespace Syndiesis.Models;

public class SelectionSpan
{
    public LinePosition SelectionStart { get; set; }
    public LinePosition SelectionEnd { get; set; }

    public bool HasSelection => !SelectionPositionSpan.IsEmpty();

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
}
