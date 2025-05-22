using Garyon.Extensions;

namespace Syndiesis.Updating;

public readonly record struct DownloadProgress(
    long DownloadedBytes, long TotalBytes)
{
    public double Progress => (double)DownloadedBytes / TotalBytes.OneOrGreater();
}
