#if DEBUG
#define FORCE_DISCOVER_UPDATE
#endif

using Octokit;
using Serilog;
using Syndiesis.Core;
using System;
using System.ComponentModel;
using Updatum;

namespace Syndiesis.Updating;

public sealed class UpdateManager
{
    private readonly UpdatumManager _updater = new(EntryApplication.AssemblyRepositoryUrl)
    {
        AssetRegexPattern = $"^Syndiesis-.*-{EntryApplication.GenericRuntimeIdentifier}\\.zip",
        DownloadProgressUpdateFrequencySeconds = 0,
#if FORCE_DISCOVER_UPDATE
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

    private State _state = State.Unchecked;

    private static readonly PropertyChangedEventArgs _propertyChangedArgs
        = new(nameof(UpdateState));

    public State UpdateState
    {
        get => _state;
        private set
        {
            _state = value;
            OnPropertyChanged(this, _propertyChangedArgs);
            UpdaterStateChanged?.Invoke(this, value);
        }
    }

    public GitReference? LatestReleaseCommit { get; private set; }
    public Release? Release => _updater.LatestRelease;
    public string? LatestVersionString => _updater.LatestReleaseTagVersionStr;

    public event EventHandler<State> UpdaterStateChanged;
    public event PropertyChangedEventHandler? UpdaterPropertyChanged;

    public UpdateManager()
    {
        InitializeEvents();
    }

    private void InitializeEvents()
    {
        _updater.PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdaterPropertyChanged?.Invoke(sender, e);
    }

    public async Task CheckForUpdates()
    {
        try
        {
            UpdateState = State.Checking;

            Log.Information($"Checking for updates (triggering on version {_updater.CurrentVersion})");
            var updateFound = await GetUpdateInfo();

            Log.Information($"Latest release found: {_updater.LatestRelease?.Name}");

            if (!updateFound)
            {
                UpdateState = State.UpToDate;
                return;
            }

            UpdateState = State.DiscoveredUpdate;

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

        UpdateState = State.Downloading;
        Log.Information($"Beginning downloading latest release from {_updater.LatestRelease?.Url}");
        _downloadedAsset = await _updater.DownloadUpdateAsync();
        UpdateState = State.ReadyToInstall;
    }

    public async Task InstallDownloadedUpdate()
    {
        if (_downloadedAsset is null)
        {
            return;
        }

        UpdateState = State.Installing;
        var result = await _updater.InstallUpdateAsync(_downloadedAsset);
        if (!result)
        {
            Log.Error("Failed to install the downloaded asset");
            UpdateState = State.InstallationFailed;
        }
    }

    public enum State
    {
        Unchecked,
        Checking,
        UpToDate,
        DiscoveredUpdate,
        Downloading,
        ReadyToInstall,
        Installing,
        InstallationFailed,
    }
}
