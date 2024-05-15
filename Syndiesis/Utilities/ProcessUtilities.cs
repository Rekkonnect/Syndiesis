using System.Diagnostics;

namespace Syndiesis.Utilities;

public static class ProcessUtilities
{
    public static void OpenUrl(string url)
    {
        Process.Start("explorer", url);
    }
}
