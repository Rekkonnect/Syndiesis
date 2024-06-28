using Avalonia;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace Syndiesis.Controls.Editor;

internal sealed class DiagnosticLineGeometryBuilder
{
    private readonly List<DrawnLine> _lines = [];

    public void AddSegment(TextView textView, TextSpan span, IBrush lineBrush)
    {
        if (span.Length is 0)
        {
            span = new TextSpan(span.Start, 1);
        }
        var segment = new SimpleSegment(span.Start, span.Length);
        AddSegment(textView, segment, lineBrush);
    }

    public void AddSegment(TextView textView, ISegment segment, IBrush lineBrush)
    {
        Debug.Assert(textView is not null);

        var rectangles = BackgroundGeometryBuilder.GetRectsForSegment(textView, segment, false);
        foreach (var rectangle in rectangles)
        {
            AddRectangle(rectangle, lineBrush);
        }
    }

    private void AddRectangle(Rect rectangle, IBrush lineBrush)
    {
        const double bottomRate = 0.14;
        const double topRate = 0.24;
        const double heightRate = topRate - bottomRate;

        var sourceBottom = rectangle.Bottom;
        var sourceHeight = rectangle.Height;
        var top = sourceBottom - sourceHeight * topRate;
        var height = sourceHeight * heightRate;

        var committedRectangle = rectangle
            .WithY(top)
            .WithHeight(height);
        CommitRectangle(committedRectangle, lineBrush);
    }

    private void CommitRectangle(Rect rectangle, IBrush lineBrush)
    {
        var line = new DrawnLine(rectangle, lineBrush);
        _lines.Add(line);
    }

    public void RenderOntoContext(DrawingContext drawingContext)
    {
        foreach (var line in _lines)
        {
            drawingContext.DrawRectangle(
                brush: line.Brush,
                pen: null,
                rect: line.Rect);
        }
    }

    public readonly record struct DrawnLine(Rect Rect, IBrush Brush);
}
