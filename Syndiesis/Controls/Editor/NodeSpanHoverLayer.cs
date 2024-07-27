using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using Microsoft.CodeAnalysis.Text;
using Syndiesis.Core;

namespace Syndiesis.Controls.Editor;

// Copied from Avalonia.Edit's SelectionLayer with minor adjustments
internal sealed class NodeSpanHoverLayer : SyndiesisTextEditorLayer
{
    public IBrush? FullSpanHoverForeground { get; set; }
    public IBrush? InnerSpanHoverForeground { get; set; }

    public NodeSpanHoverLayer(CodeEditor codeEditor)
        : base(codeEditor)
    {
        CaretWeakEventManager.PositionChanged.AddHandler(TextArea.Caret, ReceiveWeakEvent);
    }

    public override void Render(DrawingContext drawingContext)
    {
        base.Render(drawingContext);

        var selectionBorder = TextArea.SelectionBorder;

        var geoBuilder = new BackgroundGeometryBuilder
        {
            AlignToWholePixels = true,
            BorderThickness = selectionBorder?.Thickness ?? 0,
            ExtendToFullWidthAtLineEnd = false,
            CornerRadius = TextArea.SelectionCornerRadius,
        };

        var segments = CurrentHoveredSegments();
        // Order matters
        Draw(segments.InnerSpan, InnerSpanHoverForeground);
        Draw(segments.FullSpan, FullSpanHoverForeground);

        RenderBackgroundMethod.Invoke(TextView, [drawingContext, KnownLayer.Selection]);

        void Draw(SimpleSegment segment, IBrush? foreground)
        {
            if (segment.Length is 0)
                return;

            geoBuilder.AddSegment(TextView, segment);

            var geometry = geoBuilder.CreateGeometry();
            if (geometry is null)
                return;

            drawingContext.DrawGeometry(foreground, selectionBorder, geometry);
        }
    }

    private HoveredListNodeSegments CurrentHoveredSegments()
    {
        var node = CodeEditor.HoveredListNode;
        if (node is null)
            return default;

        var nodeLine = node.NodeLine;
        var associatedObject = nodeLine.AssociatedSyntaxObject;
        if (associatedObject is null)
            return default;

        var full = associatedObject.FullSpan;
        var inner = associatedObject.Span;

        int textLength = TextView.Document.TextLength;

        var fullSegment = SegmentFromSpan(full, textLength);
        var innerSegment = SegmentFromSpan(inner, textLength);
        return new(fullSegment, innerSegment);
    }

    private static SimpleSegment SegmentFromSpan(TextSpan span, int textLength)
    {
        var segment = new SimpleSegment(span.Start, span.Length);
        return segment.ConfineToBounds(textLength);
    }

    private readonly record struct HoveredListNodeSegments(
        SimpleSegment FullSpan,
        SimpleSegment InnerSpan);
}
