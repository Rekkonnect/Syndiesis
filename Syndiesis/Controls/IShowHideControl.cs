namespace Syndiesis.Controls;

public interface IShowHideControl
{
    public bool IsShowing { get; }

    public Task Show();
    public Task Hide();
}
