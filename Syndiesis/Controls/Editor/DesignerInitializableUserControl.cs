namespace Syndiesis.Controls;

public abstract class DesignerInitializableUserControl : UserControl
{
    protected void InitializeDesigner()
    {
        if (!Design.IsDesignMode)
            return;

        InitializeDesignerCore();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        InitializeDesigner();
    }

    protected abstract void InitializeDesignerCore();
}
