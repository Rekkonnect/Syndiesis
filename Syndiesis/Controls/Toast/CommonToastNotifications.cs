namespace Syndiesis.Controls.Toast;

public static class CommonToastNotifications
{
    public static async Task ShowClassic(
        ToastNotificationContainer? container,
        Color backgroundFill,
        string text,
        TimeSpan holdDuration)
    {
        if (container is null)
            return;
        
        var popup = new ToastNotificationPopup();
        popup.BackgroundFill = backgroundFill;
        popup.defaultTextBlock.Text = text;
        var animation = new BlurOpenDropCloseToastAnimation(holdDuration);
        await container.Show(popup, animation);
    }

    public static async Task ShowClassicMain(
        ToastNotificationContainer? container,
        string text,
        TimeSpan holdDuration)
    {
        await ShowClassic(container, FillColors.Main, text, holdDuration);
    }

    public static async Task ShowClassicFailure(
        ToastNotificationContainer? container,
        string text,
        TimeSpan holdDuration)
    {
        await ShowClassic(container, FillColors.Failure, text, holdDuration);
    }

    public static class FillColors
    {
        public static readonly Color Main = Color.FromUInt32(0xFF004044);
        public static readonly Color Failure = Color.FromUInt32(0xFF660030);
        public static readonly Color Warning = Color.FromUInt32(0xFF504020);
    }
}
