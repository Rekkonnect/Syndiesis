using Avalonia.Animation;
using Avalonia.Styling;
using Garyon.Extensions;
using Garyon.Objects;
using Syndiesis.Updating;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Transactions;

namespace Syndiesis.Controls.Updating;

public partial class UpdateProgressBar : UserControl
{
    private ColumnDistributor _progressBarColumnDistributor;

    public UpdateProgressBar()
    {
        InitializeComponent();
        InitializeEvents();
        InitializeDistribution();
        UpdateProgress();
        SetFlowAnimation();
    }

    [MemberNotNull(nameof(_progressBarColumnDistributor))]
    private void InitializeDistribution()
    {
        var columns = progressBarGrid.ColumnDefinitions;
        _progressBarColumnDistributor = new(
            columns[0], columns[1]);
    }

    private void InitializeEvents()
    {
        var updateManager = Singleton<UpdateManager>.Instance;
        updateManager.UpdaterPropertyChanged += HandlePropertyChanged;
        updateManager.UpdaterStateChanged += HandleStateChanged;
    }

    private void HandleStateChanged(object? sender, UpdateManager.State state)
    {
        Dispatcher.UIThread.InvokeAsync(UpdateGradientDisplay);
    }

    private void UpdateGradientDisplay()
    {
        var manager = Singleton<UpdateManager>.Instance;
        var state = manager.UpdateState;
        bool isDownloaded = state is UpdateManager.State.ReadyToInstall;
        gradientAnimationBorder.Opacity = isDownloaded ? 1 : 0;
    }

    private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(UpdateProgress);
    }

    private void UpdateProgress()
    {
        var manager = Singleton<UpdateManager>.Instance;
        var progress = manager.DownloadProgress;
        bool isDownloading = progress is not null;

        noUpdateInformationText.IsVisible = !isDownloading;
        downloadProgressTextGrid.IsVisible = isDownloading;

        if (!isDownloading)
        {
            return;
        }

        var progressRatio = progress!.Value.Progress;
        _progressBarColumnDistributor.SetProgressRatio(progressRatio);

        downloadedMegabytesText.Text = MegabyteString(progress!.Value.DownloadedBytes);
        updateMegabytesText.Text = MegabyteString(progress!.Value.TotalBytes.ZeroOrGreater());
    }

    private static string MegabyteString(long bytes)
    {
        const double megabyteSize = 1024 * 1024;
        var megabytes = bytes / megabyteSize;
        return megabytes.ToString("N2");
    }

    private void SetFlowAnimation()
    {
        Styles.Add(new Style(x => x.OfType<Border>().Name(nameof(gradientAnimationBorder)))
        {
            Animations =
            {
                CreateFlowAnimation(),
            }
        });
    }

    private static Animation CreateFlowAnimation()
    {
        var parameters = new FlowAnimationParameters
        {
            BrushStart = new(0, -1.5, RelativeUnit.Relative),
            BrushEnd = new(1, 1.5, RelativeUnit.Relative),
            Color1 = Color.FromUInt32(0xFF004B50),
            Color2 = Color.FromUInt32(0xFF007A82),
            SetterProperty = Border.BackgroundProperty,
            Duration = TimeSpan.FromSeconds(5),
            ColorGradientRatio = 0.25,
            ColorHoldRatio = 0.15,
        };
        return parameters.CompileAnimation();
    }

    private sealed record FlowAnimationParameters
    {
        public required RelativePoint BrushStart { get; init; }
        public required RelativePoint BrushEnd { get; init; }
        public required Color Color1 { get; init; }
        public required Color Color2 { get; init; }
        public required AvaloniaProperty SetterProperty { get; init; }
        public required TimeSpan Duration { get; init; }
        public required double ColorGradientRatio { get; init; }
        public required double ColorHoldRatio { get; init; }

        public Animation CompileAnimation()
        {
            var ratioPerColor = ColorGradientRatio + ColorHoldRatio;
            var fullLoopRatio = ratioPerColor * 2;
            int loops = (int)Math.Ceiling(1D / fullLoopRatio) + 1;

            const int keyframeCount = 2;
            var animation = new Animation
            {
                Duration = Duration,
                IterationCount = IterationCount.Infinite,
            };

            for (int i = 0; i < keyframeCount; i++)
            {
                var keyframeCue = i / (double)(keyframeCount - 1);
                var stops = CreateGradientStops(keyframeCue);

                var brush = new LinearGradientBrush
                {
                    StartPoint = BrushStart,
                    EndPoint = BrushEnd,
                    GradientStops = stops,
                };

                var keyframe = new KeyFrame
                {
                    Cue = new(keyframeCue),
                    Setters =
                    {
                        new Setter(SetterProperty, brush),
                    },
                };

                animation.Children.Add(keyframe);
            }

            return animation;

            GradientStops CreateGradientStops(double cue)
            {
                var stops = new GradientStops();

                var currentOffset = -(fullLoopRatio * (1 - cue));
                for (int i = 0; i < loops; i++)
                {
                    AddWithColor(Color1);
                    currentOffset += ColorGradientRatio;

                    AddWithColor(Color2);
                    currentOffset += ColorHoldRatio;

                    AddWithColor(Color2);
                    currentOffset += ColorGradientRatio;

                    AddWithColor(Color1);
                    currentOffset += ColorHoldRatio;

                    void AddWithColor(Color color)
                    {
                        stops.Add(
                            new()
                            {
                                Color = color,
                                Offset = currentOffset,
                            });
                    }
                }

                return stops;
            }
        }
    }

    private sealed record ColumnDistributor(
        ColumnDefinition Progressed,
        ColumnDefinition Remaining)
    {
        public void SetProgressRatio(double ratio)
        {
            Progressed.Width = CreateRatioLength(ratio);
            Remaining.Width = CreateRatioLength(1 - ratio);
        }

        private static GridLength CreateRatioLength(double ratio)
        {
            return new(ratio, GridUnitType.Star);
        }
    }
}
