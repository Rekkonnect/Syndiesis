using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Syndiesis.Utilities;

public static class ProcessUtilities
{
    // --- https://stackoverflow.com/a/43232486
    // |
    //  -> https://brockallen.com/2016/09/24/process-start-for-urls-on-net-core/
    //     "Fun times in this cross-platform world."
    public static void OpenUrl(string url)
    {
        try
        {
            Process.Start(url);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }
}
