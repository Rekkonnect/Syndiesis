using Microsoft.CodeAnalysis.Text;

namespace Syndiesis.Models;

public class SelectionSpan
{
    public bool HasSelection { get; set; }
    public LinePosition SelectionStart { get; set; }
    public LinePosition SelectionEnd { get; set; }

    public LinePositionSpan SelectionPositionSpan
    {
        get
        {
            return LinePositionExtensions.FromBounds(SelectionStart, SelectionEnd);
        }
    }

    public void Clear()
    {
        HasSelection = false;
        SelectionStart = default;
        SelectionEnd = default;
    }
}
