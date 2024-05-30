using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using System;
using System.Reflection;

namespace Syndiesis.Controls.Editor;

// Copied from Avalonia.Edit's SelectionLayer with minor adjustments
internal sealed class NodeSpanHoverLayer : Control
{
    private static readonly MethodInfo _renderBackgroundMethod
        = typeof(TextView)
            .GetMethod(
                "RenderBackground",
                BindingFlags.Instance | BindingFlags.NonPublic)!
            ;

    private readonly CodeEditor _codeEditor;
    private readonly TextArea _textArea;
    private readonly TextView _textView;

    public IBrush? HoverForeground { get; set; }

    public NodeSpanHoverLayer(CodeEditor codeEditor)
    {
        _codeEditor = codeEditor;
        _textArea = codeEditor.textEditor.TextArea;
        _textView = _textArea.TextView;

        IsHitTestVisible = false;

        CaretWeakEventManager.PositionChanged.AddHandler(_textArea.Caret, ReceiveWeakEvent);
        TextViewWeakEventManager.ScrollOffsetChanged.AddHandler(_textView, ReceiveWeakEvent);
    }

    private void ReceiveWeakEvent(object? sender, EventArgs e)
    {
        InvalidateVisual();
    }

    public override void Render(DrawingContext drawingContext)
    {
        base.Render(drawingContext);

        var selectionBorder = _textArea.SelectionBorder;

        var geoBuilder = new BackgroundGeometryBuilder
        {
            AlignToWholePixels = true,
            BorderThickness = selectionBorder?.Thickness ?? 0,
            ExtendToFullWidthAtLineEnd = false,
            CornerRadius = _textArea.SelectionCornerRadius,
        };

        var segment = _codeEditor.HoveredListNodeSegment;
        if (segment.Length is not 0)
        {
            geoBuilder.AddSegment(_textView, segment);

            var geometry = geoBuilder.CreateGeometry();
            if (geometry is not null)
            {
                drawingContext.DrawGeometry(HoverForeground, selectionBorder, geometry);
            }
        }

        _renderBackgroundMethod.Invoke(_textView, [drawingContext, KnownLayer.Selection]);
    }
}
