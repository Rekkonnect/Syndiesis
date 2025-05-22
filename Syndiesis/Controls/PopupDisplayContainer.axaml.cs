using Avalonia.Controls;
using Garyon.Objects;

namespace Syndiesis.Controls;

public partial class PopupDisplayContainer : UserControl, IShowHideControl
{
    public ControlShowHideHandler ShowHideHandler { get; set; }
        = Singleton<DefaultControlShowHideHandler>.Instance;

    public double BackgroundOpacity { get; set; } = 0.2;

    public Control? Popup
    {
        get => popupContainer.Content as Control;
        set => popupContainer.Content = value;
    }

    public PopupDisplayContainer()
    {
        InitializeComponent();
    }

    public async Task Show()
    {
        outerGrid.IsHitTestVisible = true;
        outerGrid.Opacity = BackgroundOpacity;
        
        await ShowHideHandler.Show(Popup);
    }

    public async Task Hide()
    {
        outerGrid.IsHitTestVisible = false;
        outerGrid.Opacity = 0;

        await ShowHideHandler.Hide(Popup);
    }
}
