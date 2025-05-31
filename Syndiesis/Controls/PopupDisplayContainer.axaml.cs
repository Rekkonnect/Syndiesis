using Avalonia.VisualTree;
using Garyon.Objects;
using Syndiesis.Views;

namespace Syndiesis.Controls;

public partial class PopupDisplayContainer : UserControl, IShowHideControl
{
    public ControlShowHideHandler ShowHideHandler { get; set; }
        = Singleton<DefaultControlShowHideHandler>.Instance;

    public double BackgroundOpacity { get; set; } = 0.5;

    public bool IsShowing { get; private set; }

    public Control? Popup
    {
        get => popupContainer.Children.FirstOrDefault();
        set
        {
            if (value is null)
            {
                popupContainer.Children.Clear();
                return;
            }

            popupContainer.Children.ClearSetValue(value);
        }
    }

    public PopupDisplayContainer()
    {
        InitializeComponent();
    }

    public async Task Show()
    {
        backgroundBorder.IsHitTestVisible = true;
        backgroundBorder.Opacity = BackgroundOpacity;
        
        await ShowHideHandler.Show(Popup);
        IsShowing = true;
    }

    public async Task Hide()
    {
        backgroundBorder.IsHitTestVisible = false;
        backgroundBorder.Opacity = 0;

        await ShowHideHandler.Hide(Popup);
        IsShowing = false;
    }

    public static PopupDisplayContainer? GetFromOuterMainViewContainer(Visual visual)
    {
        return visual.FindAncestorOfType<MainViewContainer>()?.popupContainer;
    }
}
