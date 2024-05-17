using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Syndiesis.Controls.Toast;

public sealed class BlurOpenDropCloseToastAnimation(TimeSpan holdDuration)
    : BaseToastNotificationAnimation
{
    private readonly TimeSpan _holdDuration = holdDuration;

    public override void Setup(ToastNotificationPopup popup)
    {
        popup.VerticalAlignment = VerticalAlignment.Bottom;
        popup.HorizontalAlignment = HorizontalAlignment.Center;
        popup.Padding = new(0, 0, 0, 50);
        popup.Opacity = 1;
    }

    public override async Task Animate(
        ToastNotificationPopup popup,
        CancellationToken cancellationToken)
    {
        var blur = new BlurEffect
        {
            Radius = 4,
        };
        popup.Effect = blur;

        const double initialScale = 0.925;
        var scaleTransform = new ScaleTransform(initialScale, initialScale);
        popup.RenderTransform = scaleTransform;

        var openDuration = TimeSpan.FromMilliseconds(125);
        var closeDuration = TimeSpan.FromMilliseconds(250);

        var totalProgressDuration = openDuration + _holdDuration;

        var openBlurAnimation = new Animation()
        {
            Duration = openDuration,
            Children =
            {
                new KeyFrame()
                {
                    Cue = new(0),
                    Setters =
                    {
                        new Setter(BlurEffect.RadiusProperty, 4D),
                    }
                },
                new KeyFrame()
                {
                    Cue = new(1),
                    Setters =
                    {
                        new Setter(BlurEffect.RadiusProperty, 0D),
                    }
                },
            }
        };
        var openBlurTransition = new TransitionAnimation(openBlurAnimation);

        var openBlurTask = openBlurTransition.RunAsync(blur, cancellationToken);

        var openScaleTransitionX = new DoubleTransition()
        {
            Duration = openDuration,
            Property = ScaleTransform.ScaleXProperty,
        };
        var openScaleTransitionY = new DoubleTransition()
        {
            Duration = openDuration,
            Property = ScaleTransform.ScaleYProperty,
        };

        scaleTransform.Transitions ??= new();
        scaleTransform.Transitions.Add(openScaleTransitionX);
        scaleTransform.Transitions.Add(openScaleTransitionY);

        scaleTransform.ScaleX = 1;
        scaleTransform.ScaleY = 1;

        var progressTask = popup.BeginProgressBarAsync(totalProgressDuration, cancellationToken);

        await progressTask;

        if (!cancellationToken.IsCancellationRequested)
        {
            Debug.Assert(openBlurTask.IsCompleted);
        }

        scaleTransform.Transitions.Remove(openScaleTransitionX);
        scaleTransform.Transitions.Remove(openScaleTransitionY);

        var closeBlurAnimation = new Animation()
        {
            Duration = closeDuration,
            Children =
            {
                new KeyFrame()
                {
                    Cue = new(0),
                    Setters =
                    {
                        new Setter(BlurEffect.RadiusProperty, 0D),
                    }
                },
                new KeyFrame()
                {
                    Cue = new(1),
                    Setters =
                    {
                        new Setter(BlurEffect.RadiusProperty, 2D),
                    }
                },
            }
        };
        var closeBlurTransition = new TransitionAnimation(closeBlurAnimation);

        var closeScaleTransitionX = new DoubleTransition()
        {
            Duration = closeDuration,
            Property = ScaleTransform.ScaleXProperty,
        };
        var closeScaleTransitionY = new DoubleTransition()
        {
            Duration = closeDuration,
            Property = ScaleTransform.ScaleYProperty,
        };

        scaleTransform.Transitions.Add(closeScaleTransitionX);
        scaleTransform.Transitions.Add(closeScaleTransitionY);

        scaleTransform.ScaleX = initialScale;
        scaleTransform.ScaleY = initialScale;

        var closeOpacityAnimation = new Animation()
        {
            Duration = closeDuration,
            Children =
            {
                new KeyFrame()
                {
                    Cue = new(0),
                    Setters =
                    {
                        new Setter(Control.OpacityProperty, 1D),
                    }
                },
                new KeyFrame()
                {
                    Cue = new(1),
                    Setters =
                    {
                        new Setter(Control.OpacityProperty, 0D),
                    }
                },
            }
        };
        var closeOpacityTransition = new TransitionAnimation(closeOpacityAnimation);

        var paddingTransition = new ThicknessTransition()
        {
            Duration = closeDuration,
            Property = ToastNotificationPopup.PaddingProperty,
        };

        popup.Transitions ??= [];
        popup.Transitions.Add(paddingTransition);

        var previousPadding = popup.Padding;
        var closePadding = previousPadding.WithBottom(previousPadding.Bottom - 12);
        popup.Padding = closePadding;

        var closeBlurTask = closeBlurTransition.RunAsync(blur, default);
        var closeOpacityTask = closeOpacityTransition.RunAsync(popup, default);

        await closeBlurTask;
        await closeOpacityTask;

        scaleTransform.Transitions.Remove(closeScaleTransitionX);
        scaleTransform.Transitions.Remove(closeScaleTransitionY);

        popup.Transitions.Remove(paddingTransition);
    }
}
