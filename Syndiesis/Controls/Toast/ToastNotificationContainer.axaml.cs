using Avalonia.Input;
using Garyon.Objects;

namespace Syndiesis.Controls.Toast;

public partial class ToastNotificationContainer : UserControl
{
    private readonly CancellationTokenFactory _cancellationTokenFactory = new();

    public ToastNotificationContainer()
    {
        InitializeComponent();
    }

    public async Task Show(
        ToastNotificationPopup popup,
        BaseToastNotificationAnimation animation)
    {
        _cancellationTokenFactory.Cancel();

        var currentToken = _cancellationTokenFactory.CurrentToken;
        await RetainAnimate(popup, animation, currentToken);
    }

    private async Task RetainAnimate(
        ToastNotificationPopup popup,
        BaseToastNotificationAnimation animation,
        CancellationToken cancellationToken)
    {
        animation.Setup(popup);
        popup.PointerPressed += HandlePointerPressed;
        toastContainer.Children.Add(popup);
        await animation.Animate(popup, cancellationToken);
        popup.PointerPressed -= HandlePointerPressed;
        toastContainer.Children.Remove(popup);
    }

    private void HandlePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _cancellationTokenFactory.Cancel();
    }

    public static ToastNotificationContainer? GetFromOuterMainViewContainer()
    {
        var mainView = App.Current.GetMainViewContainer();
        return mainView?.ToastNotificationContainer;
    }
}
