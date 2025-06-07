using Serilog;
using Syndiesis.Controls.Toast;

namespace Syndiesis;

public static class LowEndDeviceDetection
{
    private static volatile bool _notifiedLowEndDevice;

    public static void RaiseLowEndDevice()
    {
        if (_notifiedLowEndDevice)
            return;

        const string message = "Low-end device detected, are you sure this application is running smoothly?";
        Log.Warning(message);
        _notifiedLowEndDevice = true;

        var toastContainer = ToastNotificationContainer.GetFromOuterMainViewContainer();
        _ = CommonToastNotifications.ShowClassic(
            toastContainer,
            CommonToastNotifications.FillColors.Warning,
            message,
            TimeSpan.FromSeconds(5));
    }
}
