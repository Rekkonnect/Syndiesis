using Avalonia.Controls;
using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor;

public partial class QuickInfoDiagnosticItem : UserControl
{
    public QuickInfoDiagnosticItem()
    {
        InitializeComponent();
    }

    public void LoadDiagnostic(Diagnostic diagnostic)
    {
        var image = ImageForDiagnostic(diagnostic);
        diagnosticIcon.Source = image?.Source;
        diagnosticCodeText.Text = diagnostic.Id;
        diagnosticMessageText.Text = diagnostic.GetMessage();
    }

    private static Image? ImageForDiagnostic(Diagnostic diagnostic)
    {
        var severity = diagnostic.Severity;
        return ImageForDiagnosticSeverity(severity);
    }

    private static Image? ImageForDiagnosticSeverity(DiagnosticSeverity severity)
    {
        var resourceManager = App.Current.ResourceManager;
        return severity switch
        {
            DiagnosticSeverity.Error => resourceManager.DiagnosticErrorImage,
            DiagnosticSeverity.Warning => resourceManager.DiagnosticWarningImage,
            DiagnosticSeverity.Info => resourceManager.DiagnosticSuggestionImage,
            // no dedicated image for now
            DiagnosticSeverity.Hidden => resourceManager.DiagnosticSuggestionImage,
            _ => null,
        };
    }
}
