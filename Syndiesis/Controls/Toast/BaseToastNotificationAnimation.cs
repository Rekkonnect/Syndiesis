namespace Syndiesis.Controls.Toast;

public abstract class BaseToastNotificationAnimation
{
    public virtual void Setup(ToastNotificationPopup popup) { }

    public abstract Task Animate(
        ToastNotificationPopup popup,
        CancellationToken cancellationToken);
}
