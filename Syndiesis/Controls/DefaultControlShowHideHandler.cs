using Avalonia.Controls;

namespace Syndiesis.Controls;

public class DefaultControlShowHideHandler
    : ControlShowHideHandler
{
    public override async Task Show(Control? control)
    {
        if (control is IShowHideControl showHide)
        {
            await showHide.Show();
            return;
        }
    }

    public override async Task Hide(Control? control)
    {
        if (control is IShowHideControl showHide)
        {
            await showHide.Hide();
            return;
        }
    }
}
