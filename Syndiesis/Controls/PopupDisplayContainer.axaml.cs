using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Garyon.Objects;
using Syndiesis.Views;
using System.Linq;

namespace Syndiesis.Controls;

public partial class PopupDisplayContainer : UserControl, IShowHideControl
{
    public ControlShowHideHandler ShowHideHandler { get; set; }
        = Singleton<DefaultControlShowHideHandler>.Instance;

    public double BackgroundOpacity { get; set; } = 0.35;

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
    }

    public async Task Hide()
    {
        backgroundBorder.IsHitTestVisible = false;
        backgroundBorder.Opacity = 0;

        await ShowHideHandler.Hide(Popup);
    }

    public static PopupDisplayContainer? GetFromOuterMainViewContainer(Visual visual)
    {
        return visual.FindAncestorOfType<MainViewContainer>()?.popupContainer;
    }
}
