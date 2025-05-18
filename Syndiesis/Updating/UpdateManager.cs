#if DEBUG
#define FORCE_UPDATE_CHECK
#define FORCE_DOWNLOAD
#define FORCE_INSTALL
#endif

using Serilog;
using System;
using System.Threading.Tasks;
using Updatum;

namespace Syndiesis.Updating;

public sealed class UpdateManager
{
    private readonly UpdatumManager _updater = new(EntryApplication.AssemblyRepositoryUrl)
    {
        AssetRegexPattern = $"^Syndiesis-.*-{EntryApplication.GenericRuntimeIdentifier}\\.zip",
        DownloadProgressUpdateFrequencySeconds = 0,
#if FORCE_UPDATE_CHECK
        CurrentVersion = Version.Parse("1.0.0"),
#endif
    };

    private UpdatumDownloadedAsset? _downloadedAsset;

    public DownloadProgress? DownloadProgress
    {
        get
        {
            if (_updater.DownloadSizeBytes is 0)
            {
                return null;
            }
            return new(_updater.DownloadedBytes, _updater.DownloadSizeBytes);
        }
    }

    public UpdatumState UpdatumState => _updater.State;

    public async Task CheckForUpdates()
    {
        try
        {
            Log.Information($"Checking for updates (triggering on version {_updater.CurrentVersion})");
            var updateFound = await _updater.CheckForUpdatesAsync();

            Log.Information($"Latest release found: {_updater.LatestRelease?.Name}");

            if (!updateFound)
            {
                Log.Information("No update was found");
            }

#if FORCE_DOWNLOAD
            await EnsureUpdateDownloaded();
#endif
        }
        catch (Exception ex)
        {
            App.Current.ExceptionListener.HandleException(
                ex, "An exception occurred while trying to check and install the update");
        }
    }

    public async Task EnsureUpdateDownloaded()
    {
        if (_downloadedAsset is not null)
        {
            return;
        }

        Log.Information($"Beginning downloading latest release from {_updater.LatestRelease?.Url}");
        _downloadedAsset = await _updater.DownloadUpdateAsync();
        
#if FORCE_INSTALL
        await InstallDownloadedUpdate();
#endif
    }

    public async Task InstallDownloadedUpdate()
    {
        if (_downloadedAsset is null)
        {
            return;
        }

        var result = await _updater.InstallUpdateAsync(_downloadedAsset);
        if (!result)
        {
            Log.Error("Failed to install the downloaded asset");
        }
    }
}
