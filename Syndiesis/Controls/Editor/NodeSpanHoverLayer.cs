using Avalonia.Media;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;

namespace Syndiesis.Controls.Editor;

// Copied from Avalonia.Edit's SelectionLayer with minor adjustments
internal sealed class NodeSpanHoverLayer : SyndiesisTextEditorLayer
{
    public IBrush? HoverForeground { get; set; }

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

        var segment = CodeEditor.HoveredListNodeSegment;
        if (segment.Length is not 0)
        {
            geoBuilder.AddSegment(TextView, segment);

            var geometry = geoBuilder.CreateGeometry();
            if (geometry is not null)
            {
                drawingContext.DrawGeometry(HoverForeground, selectionBorder, geometry);
            }
        }

        RenderBackgroundMethod.Invoke(TextView, [drawingContext, KnownLayer.Selection]);
    }
}
