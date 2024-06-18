using Avalonia.Threading;
using Syndiesis.Controls;
using Syndiesis.Controls.Editor;

namespace Syndiesis;

public class ColorizationPreferences
{
    public RoslynColorizer.ColorizationStyles? ColorizationStyles;

    public ColorizationPreferences()
    {
        Dispatcher.UIThread.ExecuteOrDispatch(Initialize);

        void Initialize()
        {
            ColorizationStyles = new();
        }
    }
}
