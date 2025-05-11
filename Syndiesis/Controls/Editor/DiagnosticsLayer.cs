using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Syndiesis.Core;
using Syndiesis.InternalGenerators.Core;
using System;

namespace Syndiesis.Controls.Editor;

public sealed partial class DiagnosticsLayer : SyndiesisTextEditorLayer
{
    public bool Enabled;

    public DiagnosticsLayer(CodeEditor codeEditor)
        : base(codeEditor)
    {
        codeEditor.AnalysisCompleted += HandleAnalysisCompleted;
    }

    private void HandleAnalysisCompleted()
    {
        Dispatcher.UIThread.InvokeAsync(InvalidateVisual);
    }

    public override void Render(DrawingContext drawingContext)
    {
        base.Render(drawingContext);

        if (!Enabled)
            return;

        var diagnostics = CodeEditor.CompilationSource?.CurrentSource.Diagnostics;
        if (diagnostics is null)
            return;

        var firstLineIndex = Math.Max(TextView.GetFirstVisibleLine() - 1, 0);
        var lastLineIndex = TextView.GetLastVisibleLine();

        var geometryBuilder = new DiagnosticLineGeometryBuilder();
        for (int i = firstLineIndex; i <= lastLineIndex; i++)
        {
            var line = TextView.GetVisualLine(i);
            var lineDiagnostics = diagnostics.DiagnosticsForLine(i);

            var intervalList = DiagnosticIntervalList.ForDiagnostics(lineDiagnostics);

            foreach (var diagnostic in intervalList.Entries)
            {
                var severity = diagnostic.Severity;
                var diagnosticLineSpan = diagnostic.Span;
                var brush = Styles.BrushForDiagnosticSeverity(severity);
                geometryBuilder.AddSegment(TextView, diagnosticLineSpan, brush.Brush);
            }
        }

        geometryBuilder.RenderOntoContext(drawingContext);
    }

    private TextSpan SpanForLine(LinePositionSpan lineSpan, int line, int lineLength)
    {
        var start = lineSpan.Start;
        var end = lineSpan.End;

        int min = 0;
        int max = lineLength;

        if (start.Line == line)
            min = start.Character;

        if (end.Line == line)
            max = end.Character;

        return TextSpan.FromBounds(min, max);
    }
}

partial class DiagnosticsLayer
{
    public static new DecorationStyles Styles
        => AppSettings.Instance.ColorizationPreferences.DiagnosticStyles!;

    // Defaults from VS' color scheme
    [SolidColor("OtherError", 0xFFCA79EC)]
    [SolidColor("Error", 0xFFFC3E36)]
    [SolidColor("Warning", 0xFF95DB7D)]
    [SolidColor("Information", 0xFF55AAFF)]
    [SolidColor("Hidden", 0xFF505050)]
    public sealed partial class DecorationStyles
    {
        public ILazilyUpdatedBrush BrushForDiagnosticSeverity(DiagnosticSeverity severity)
        {
            return severity switch
            {
                DiagnosticSeverity.Error => ErrorBrush,
                DiagnosticSeverity.Warning => WarningBrush,
                DiagnosticSeverity.Info => InformationBrush,
                DiagnosticSeverity.Hidden => HiddenBrush,
                _ => OtherErrorBrush,
            };
        }
    }
}
