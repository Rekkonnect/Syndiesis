using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Syndiesis.Core;

namespace Syndiesis.Controls;

public partial class LanguageVersionDropDown : UserControl
{
    public event Action<RoslynLanguageVersion>? LanguageVersionChanged;

    public LanguageVersionDropDown()
    {
        InitializeComponent();
        InitializeEvents();
    }

    private void InitializeEvents()
    {
        envelope.PointerPressed += HandlePointerPressed;
        items.ItemClicked += HandleItemClicked;
    }

    private void HandleItemClicked(object? sender, RoutedEventArgs e)
    {
        var item = (LanguageVersionDropDownItem)sender!;
        var version = item.Version;
        LanguageVersionChanged?.Invoke(version);
        DisplayVersion(version);
        var flyout = Flyout.GetAttachedFlyout(this)!;
        flyout.Hide();
    }

    public void DisplayVersion(RoslynLanguageVersion version)
    {
        envelope.DisplayVersion(version);
        items.SetVersion(version);
    }

    private void HandlePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        var attached = Flyout.GetAttachedFlyout(this)!;
        ToggleFlyout(attached, this);
        e.Handled = false;
    }

    private static void ToggleFlyout(FlyoutBase attached, Control control)
    {
        if (attached.IsOpen)
        {
            attached.Hide();
        }
        else
        {
            attached.ShowAt(control);
        }
    }
}
