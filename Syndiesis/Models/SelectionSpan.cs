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
            if (SelectionStart < SelectionEnd)
            {
                return new(SelectionEnd, SelectionStart);
            }
            else
            {
                return new(SelectionStart, SelectionEnd);
            }
        }
    }

    public void Clear()
    {
        HasSelection = false;
        SelectionStart = default;
        SelectionEnd = default;
    }
}
