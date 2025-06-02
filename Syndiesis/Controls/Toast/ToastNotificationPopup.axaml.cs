using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Styling;

namespace Syndiesis.Controls.Toast;

public partial class ToastNotificationPopup : UserControl
{
    public Color BackgroundFill
    {
        set
        {
            var brush = new SolidColorBrush(value);
            backgroundFill.Fill = brush;
            var progressBarColor = value.TransformHsv(new(Value: 0.4));
            progressBar.ProgressBarBrush = new SolidColorBrush(progressBarColor);
        }
    }

    public ToastNotificationPopup()
    {
        InitializeComponent();
        // Force set from here to ensure a correct fade
        BackgroundFill = CommonToastNotifications.FillColors.Main;
    }

    public async Task BeginProgressBarAsync(TimeSpan totalDuration, CancellationToken cancellationToken)
    {
        progressBar.MinValue = 0;
        progressBar.MaxValue = 1;
        var progressBarTransition = CreateProgressBarTransitionAnimation(totalDuration);
        await progressBarTransition.RunAsync(progressBar, cancellationToken);
    }

    private static TransitionAnimation CreateProgressBarTransitionAnimation(TimeSpan totalDuration)
    {
        return new(CreateProgressBarAnimation(totalDuration));
    }

    private static Animation CreateProgressBarAnimation(TimeSpan totalDuration)
    {
        return new Animation
        {
            Duration = totalDuration,
            Easing = new LinearEasing(),
            Children =
            {
                new KeyFrame()
                {
                    Cue = new Cue(0),
                    Setters =
                    {
                        new Setter(ToastProgressBar.CurrentValueProperty, 1D),
                    }
                },
                new KeyFrame()
                {
                    Cue = new Cue(1),
                    Setters =
                    {
                        new Setter(ToastProgressBar.CurrentValueProperty, 0D),
                    }
                }
            }
        };
    }
}
