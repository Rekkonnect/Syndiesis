using Avalonia.Controls;

namespace Syndiesis.Controls;

public abstract class ControlShowHideHandler
{
    public abstract Task Show(Control? control);
    public abstract Task Hide(Control? control);
}
