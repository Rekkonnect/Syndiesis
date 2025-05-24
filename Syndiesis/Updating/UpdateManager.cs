#if DEBUG
#define FORCE_UPDATE_CHECK
#endif

using Octokit;
using Serilog;
using Syndiesis.Core;
using System;
using System.ComponentModel;
using System.Linq;
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

    public UpdatumState UpdateState => _updater.State;

    public bool HasPendingUpdate => _downloadedAsset is not null || _updater.IsUpdateAvailable;

    public GitReference? LatestReleaseCommit { get; private set; }
    public Release? Release => _updater.LatestRelease;
    public string? LatestVersionString => _updater.LatestReleaseTagVersionStr;

    public event PropertyChangedEventHandler? UpdaterPropertyChanged;

    public async Task CheckForUpdates()
    {
        try
        {
            _updater.PropertyChanged += UpdaterPropertyChanged;

            Log.Information($"Checking for updates (triggering on version {_updater.CurrentVersion})");
            var updateFound = await GetUpdateInfo();

            Log.Information($"Latest release found: {_updater.LatestRelease?.Name}");

            if (!updateFound)
            {
                Log.Information("No update was found");
            }

            if (AppSettings.Instance.UpdateOptions.AutoDownloadUpdates)
            {
                await EnsureUpdateDownloaded();
            }
        }
        catch (Exception ex)
        {
            App.Current.ExceptionListener.HandleException(
                ex, "An exception occurred while trying to check and install the update");
        }
    }

    private async Task<bool> GetUpdateInfo()
    {
        var found = await _updater.CheckForUpdatesAsync();

        if (found)
        {
            var release = _updater.LatestRelease!;
            var tag = await _updater.GithubClient.GetTagFromRelease(
                release, owner: _updater.Owner, repository: _updater.Repository);

            var releaseCommit = tag.Commit;
            LatestReleaseCommit = releaseCommit;
        }

        return found;
    }

    public async Task EnsureUpdateDownloaded()
    {
        if (_downloadedAsset is not null)
        {
            return;
        }

        Log.Information($"Beginning downloading latest release from {_updater.LatestRelease?.Url}");
        _downloadedAsset = await _updater.DownloadUpdateAsync();
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
