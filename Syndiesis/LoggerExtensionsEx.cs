using Serilog;

namespace Syndiesis;

public static class LoggerExtensionsEx
{
    public const string DefaultOutputTemplate = "{Timestamp:[yyyy-MM-dd] [HH:mm:ss.fff zzz]} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

    public static void LogMethodInvocation(string name)
    {
        Log.Information($"{name} invoked");
    }

    private static volatile bool _loggedLowEndDevice;

    public static void LogLowEndDevice()
    {
        if (_loggedLowEndDevice)
            return;

        Log.Warning("Low-end device detected, are you sure this application is running smoothly?");
        _loggedLowEndDevice = true;
    }
}
