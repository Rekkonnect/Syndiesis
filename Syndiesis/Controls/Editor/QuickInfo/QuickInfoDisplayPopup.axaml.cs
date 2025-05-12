using Avalonia;
using Avalonia.Controls;
using Garyon.Functions;
using Microsoft.CodeAnalysis;
using Syndiesis.Core;
using System.Collections.Immutable;
using System.Linq;
using CodeAnalysisLocation = Microsoft.CodeAnalysis.Location;

namespace Syndiesis.Controls.Editor.QuickInfo;

public partial class QuickInfoDisplayPopup : DesignerInitializableUserControl
{
    private Point _pointerOrigin;
    
    public HybridSingleTreeCompilationSource? CompilationSource { get; set; }

    public QuickInfoDisplayPopup()
    {
        InitializeComponent();
    }

    private void UpdateSplitterVisibility()
    {
        splitter.IsVisible = symbolInfoPanel.Children.Count > 0
            && diagnosticsPanel.Children.Count > 0;
    }

    protected override void InitializeDesignerCore()
    {
        ImmutableArray<Diagnostic> exampleDiagnostics =
        [
            DiagnosticDescriptors.ExampleError,
            DiagnosticDescriptors.ExampleWarning,
            DiagnosticDescriptors.ExampleInfo,
        ];

        SetDiagnostics(exampleDiagnostics);
    }

    public void SetSymbols(ImmutableArray<SymbolHoverContext> symbols)
    {
        var symbolItems = symbols
            .Select(CreateSymbolItem)
            .ToArray();

        CommonAvaloniaExtensions.ClearSetValues<Control>(symbolInfoPanel.Children, symbolItems);
        UpdateSplitterVisibility();
    }

    public void SetDiagnostics(ImmutableArray<Diagnostic> diagnostics)
    {
        var diagnosticItems = diagnostics
            .Select(CreateDiagnosticItem)
            .Where(Predicates.NotNull)
            .ToArray();

        CommonAvaloniaExtensions.ClearSetValues<Control>(diagnosticsPanel.Children, diagnosticItems!);
        UpdateSplitterVisibility();
    }

    public bool IsEmpty()
    {
        return diagnosticsPanel.Children.Count is 0
            && symbolInfoPanel.Children.Count is 0;
    }
    
    public void SetPointerOrigin(Point point)
    {
        _pointerOrigin = point;
        PreparePosition();
    }

    public void PreparePosition()
    {
        const double snapMargin = 5;

        var bounds = (Parent as Control)!.Bounds.Size;

        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        Margin = new(0, 0, 0, 0);

        outerBorder.InvalidateMeasure();
        outerBorder.Measure(bounds);
        var thisSize = outerBorder.DesiredSize + outerBorder.Margin;

        var origin = _pointerOrigin;

        var targetMargin = new Thickness(origin.X, origin.Y, 0, 0);

        if (bounds.Width >= thisSize.Width)
        {
            if (origin.X + thisSize.Width >= bounds.Width)
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;

                var rightMargin = bounds.Width - origin.X;
                // if we extend too far rightwards, ignore any right margin
                // and prefer snapping to the right
                if ((origin.X - thisSize.Width) < snapMargin)
                {
                    rightMargin = snapMargin;
                }

                targetMargin = targetMargin
                    .WithLeft(snapMargin)
                    .WithRight(rightMargin)
                    ;
            }
        }

        if (bounds.Height >= thisSize.Height)
        {
            if (origin.Y + thisSize.Height >= bounds.Height)
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom;

                var bottomMargin = bounds.Height - origin.Y;
                // if we extend too far upwards, ignore any bottom margin
                // and prefer snapping to the bottom
                if ((origin.Y - thisSize.Height) < snapMargin)
                {
                    bottomMargin = snapMargin;
                }

                targetMargin = targetMargin
                    .WithTop(snapMargin)
                    .WithBottom(bottomMargin)
                    ;
            }
        }

        Margin = targetMargin;
    }

    private QuickInfoSymbolItem CreateSymbolItem(SymbolHoverContext context)
    {
        return QuickInfoSymbolItem.CreateForSymbolAndCompilation(context, CompilationSource);
    }

    private static QuickInfoDiagnosticItem? CreateDiagnosticItem(Diagnostic diagnostic)
    {
        if (diagnostic.IsSuppressed)
            return null;

        var item = new QuickInfoDiagnosticItem();
        item.LoadDiagnostic(diagnostic);
        return item;
    }

    private static class DiagnosticDescriptors
    {
        public static Diagnostic ExampleError { get; } = Diagnostic.Create(
            new DiagnosticDescriptor(
                "S#1010",
                "Example Error",
                "This is an example error",
                "Example",
                DiagnosticSeverity.Error,
                true),
            CodeAnalysisLocation.None);

        public static Diagnostic ExampleWarning { get; } = Diagnostic.Create(
            new DiagnosticDescriptor(
                "S#2020",
                "Example Warning",
                "This is an example warning",
                "Example",
                DiagnosticSeverity.Warning,
                true),
            CodeAnalysisLocation.None);

        public static Diagnostic ExampleInfo { get; } = Diagnostic.Create(
            new DiagnosticDescriptor(
                "S#3030",
                "Example Info",
                "This is an example information",
                "Example",
                DiagnosticSeverity.Info,
                true),
            CodeAnalysisLocation.None);
    }
}
