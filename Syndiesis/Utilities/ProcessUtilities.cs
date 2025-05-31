using System.Runtime.InteropServices;

namespace Syndiesis.Utilities;

public static class ProcessUtilities
{
    // --- https://stackoverflow.com/a/43232486
    // |
    //  -> https://brockallen.com/2016/09/24/process-start-for-urls-on-net-core/
    //     "Fun times in this cross-platform world."
    public static Process OpenUrl(string url)
    {
        try
        {
            return Process.Start(url);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                var startInfo = new ProcessStartInfo(url) { UseShellExecute = true };
                return Process.Start(startInfo)!;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }

    // With the help of ChatGPT
    public static Process ShowFileInFileViewer(string filePath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Process.Start("explorer.exe", $"/select,\"{filePath}\"");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Process.Start("open", $"-R \"{filePath}\"");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Process.Start("xdg-open", filePath);
        }
        else
        {
            throw new NotSupportedException("Operating system not supported");
        }
    }

    // With the help of ChatGPT
    public static Process ShowDirectoryInFileViewer(string directoryPath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Process.Start("explorer.exe", directoryPath);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Process.Start("open", directoryPath);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Process.Start("xdg-open", directoryPath);
        }
        else
        {
            throw new NotSupportedException("Operating system not supported");
        }
    }

    public static void AwaitProcessInitialized(this Process process)
    {
        try
        {
            process.WaitForInputIdle();
        }
        catch (InvalidOperationException ex)
        {
            App.Current.ExceptionListener.HandleException(
                ex,
                $"""
                Could not await for input idle on the given process
                Process info:
                  ID: {process.Id} - Name: {process.ProcessName}
                  Has exited: {process.HasExited}
                """);
        }
    }
}
