using Syndiesis.Controls;
using Syndiesis.Controls.Editor;

namespace Syndiesis;

public class ColorizationPreferences
{
    public RoslynColorizer.ColorizationStyles? ColorizationStyles;
    public DiagnosticsLayer.DecorationStyles? DiagnosticStyles;

    public ColorizationPreferences()
    {
        Dispatcher.UIThread.ExecuteOrDispatch(Initialize);

        void Initialize()
        {
            ColorizationStyles = new();
            DiagnosticStyles = new();
        }
    }
}
