using Serilog;

namespace Syndiesis;

public static class LoggerExtensionsEx
{
    public const string DefaultOutputTemplate = "{Timestamp:[yyyy-MM-dd] [HH:mm:ss.fff zzz]} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

    public static void LogMethodInvocation(string name)
    {
        Log.Information($"{name} invoked");
    }
}
