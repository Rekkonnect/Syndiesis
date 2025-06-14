﻿using Avalonia.Controls.Documents;
using Garyon.Objects;
using Syndiesis.Updating;
using Syndiesis.Utilities;
using Syndiesis.Views;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Syndiesis.Controls.Updating;

public partial class UpdatePopup : UserControl, IShowHideControl
{
    private const double _shortMainButtonHeight = 50;
    private const double _tallMainButtonHeight = 72;

    private const double _cancelButtonOffset = 20;
    private const double _mainButtonTextOffset = 18;
    private const double _mainButtonSecondaryTextOffset = _mainButtonTextOffset + 2;

    private Run _progressRun;

    public bool IsShowing { get; private set; }

    public UpdatePopup()
    {
        InitializeComponent();
        InitializeRuns();
        InitializeEvents();

        InitializeStates();
    }

    private void InitializeStates()
    {
        UpdateStateBasedLayout();
    }

    [MemberNotNull(nameof(_progressRun))]
    private void InitializeRuns()
    {
        progressPercentageText.Inlines =
        [
            _progressRun = new Run
            {
                Text = "0.0",
                FontSize = 18,
                Foreground = new SolidColorBrush(0xFF91EDF2),
            },
            new Run
            {
                Text = "%",
                FontSize = 16,
                Foreground = new SolidColorBrush(0xFF8EB0B2),
            },
        ];
    }

    private void InitializeEvents()
    {
        var manager = Singleton<UpdateManager>.Instance;
        manager.UpdaterPropertyChanged += HandlePropertyChanged;
        manager.UpdaterStateChanged += HandleUpdateStateChanged;
        mainButton.Click += OnMainButtonClicked;
        cancelButton.Click += OnCancelButtonClicked;
        viewOnGitHubButton.AttachAsyncClick(OnViewOnGitHubButtonClicked);
    }

    private void OnViewOnGitHubButtonClicked()
    {
        ProcessUtilities.OpenUrl(KnownConstants.GitHubReleasesLink)
            .AwaitProcessInitialized();
    }

    private void OnCancelButtonClicked(object? sender, RoutedEventArgs e)
    {
        Task.Run(CancelUpdate);
    }

    private void CancelUpdate()
    {
        var manager = Singleton<UpdateManager>.Instance;
        manager.UpdateDownloadCancellationTokenSource.Cancel();
    }

    private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(InvalidateArrange);
    }

    private void HandleUpdateStateChanged(object? sender, UpdateManager.State state)
    {
        Dispatcher.UIThread.InvokeAsync(UpdateStateBasedLayout);
    }

    protected override void ArrangeCore(Rect finalRect)
    {
        UpdateTexts();
        base.ArrangeCore(finalRect);
    }

    private void UpdateStateBasedLayout()
    {
        var manager = Singleton<UpdateManager>.Instance;
        var state = manager.UpdateState;

        NeedsCancelButton(state)
            .SwitchInvoke(
                ShowCancelButton,
                HideCancelButton);

        NeedsTallMainButtonHeight(state)
            .SwitchInvoke(
                MakeTallMainButton,
                MakeShortMainButton);
    }

    private bool _isShowingCancel = false;

    private void ShowCancelButton()
    {
        if (_isShowingCancel)
            return;

        cancelBorder.Opacity = 1;
        cancelBorder.Margin = cancelBorder.Margin.WithRight(0);

        _isShowingCancel = true;
    }

    private void HideCancelButton()
    {
        if (!_isShowingCancel)
            return;

        cancelBorder.Opacity = 0;
        cancelBorder.Margin = cancelBorder.Margin.WithRight(-_cancelButtonOffset);

        _isShowingCancel = false;
    }

    private bool _hasTallMainButton = false;

    private void MakeTallMainButton()
    {
        if (_hasTallMainButton)
            return;

        mainButtonBorder.Height = _tallMainButtonHeight;
        mainButtonText.Margin = mainButtonText.Margin.OffsetBottom(_mainButtonTextOffset);
        installationHelpText.Margin = installationHelpText.Margin.OffsetTop(_mainButtonSecondaryTextOffset);
        installationHelpText.Opacity = 1;

        _hasTallMainButton = true;
    }

    private void MakeShortMainButton()
    {
        if (!_hasTallMainButton)
            return;

        mainButtonBorder.Height = _shortMainButtonHeight;
        mainButtonText.Margin = mainButtonText.Margin.OffsetBottom(-_mainButtonTextOffset);
        installationHelpText.Margin = installationHelpText.Margin.OffsetTop(-_mainButtonSecondaryTextOffset);
        installationHelpText.Opacity = 0;

        _hasTallMainButton = false;
    }

    private static bool NeedsCancelButton(UpdateManager.State state)
    {
        return state is UpdateManager.State.Downloading;
    }

    private static bool NeedsTallMainButtonHeight(UpdateManager.State state)
    {
        return state
            is UpdateManager.State.ReadyToInstall
            or UpdateManager.State.InstallationFailed
            ;
    }

    private const string _installationFailedMainButtonClass = "installationFailed";

    private void UpdateTexts()
    {
        var manager = Singleton<UpdateManager>.Instance;
        var downloadProgress = manager.DownloadProgress;
        var progress = downloadProgress?.Progress ?? 0;

        var updateState = manager.UpdateState;

        mainButtonText.Text = GetMainButtonText();
        mainButton.IsEnabled = IsMainButtonActive();
        progressPercentageText.IsVisible = updateState is UpdateManager.State.Downloading;
        _progressRun.Text = (progress * 100).ToString("N1");
        installationHelpText.Text = GetSecondaryButtonText();

        bool failed = updateState is UpdateManager.State.InstallationFailed;
        mainButton.Classes.Set(
            _installationFailedMainButtonClass,
            failed)
            ;

        // The selector won't pick up this text block probably because at the
        // time of the arrangement it has an opacity of 0, so we have to manually
        // change its color
        installationHelpText.Foreground = new SolidColorBrush(
            InstallationHelpTextColor(failed));

        return;

        string GetMainButtonText()
        {
            return updateState switch
            {
                UpdateManager.State.Unchecked => "Check for updates",
                UpdateManager.State.Checking => "Checking for update",
                UpdateManager.State.UpToDate => "Up-to-date",
                UpdateManager.State.DiscoveredUpdate => "Download update",
                UpdateManager.State.Downloading => $"Downloading update...",
                UpdateManager.State.ReadyToInstall => "Install update",
                UpdateManager.State.Installing => "Installing update",
                UpdateManager.State.InstallationFailed => "Installation failed",

                _ => "Unknown update state",
            };
        }

        string GetSecondaryButtonText()
        {
            return updateState switch
            {
                UpdateManager.State.ReadyToInstall
                    => "The program will close and automatically restart.",
                UpdateManager.State.InstallationFailed
                    => "Some error occurred, click to try again.",

                _ => string.Empty,
            };
        }

        static uint InstallationHelpTextColor(bool failedInstallation)
        {
            return failedInstallation ? 0xFF996E5Cu : 0xFF3D9499u;
        }
    }

    private void OnMainButtonClicked(object? sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(HandleMainButtonClick);
    }

    private void HandleMainButtonClick()
    {
        var manager = Singleton<UpdateManager>.Instance;
        switch (manager.UpdateState)
        {
            case UpdateManager.State.Unchecked:
                Task.Run(manager.CheckForUpdates);
                break;

            case UpdateManager.State.DiscoveredUpdate:
                Task.Run(manager.EnsureUpdateDownloaded);
                break;

            case UpdateManager.State.ReadyToInstall:
            case UpdateManager.State.InstallationFailed:
                Task.Run(manager.InstallDownloadedUpdate);
                break;

            default:
                throw new InvalidOperationException(
                    "The button should not do anything in these states");
        }
    }

    private bool IsMainButtonActive()
    {
        var manager = Singleton<UpdateManager>.Instance;
        return manager.UpdateState
            is UpdateManager.State.Unchecked
            or UpdateManager.State.DiscoveredUpdate
            or UpdateManager.State.ReadyToInstall
            or UpdateManager.State.InstallationFailed
            ;
    }

    public Task Show()
    {
        Opacity = 1;
        Margin = default;
        IsShowing = true;
        return Task.CompletedTask;
    }

    public Task Hide()
    {
        var targetOutermostParent = this.NearestAncestorOfType<MainViewContainer>()!;
        var targetBar = targetOutermostParent.TitleBar;
        var topLeftOffset = this.TranslatePoint(default, targetBar)!.Value;
        var targetHeight = targetBar.Height;
        var choppedHeight = Height - targetHeight;
        var relativeTopMargin = topLeftOffset.Y;
        var targetMargin = Margin
            .WithTop(-relativeTopMargin)
            .WithBottom(choppedHeight + relativeTopMargin);

        Margin = targetMargin;
        Opacity = 0;
        Height = targetHeight;
        Width = targetOutermostParent.Bounds.Width;

        innerContent.Opacity = 0;
        IsShowing = false;
        return Task.CompletedTask;
    }
}
