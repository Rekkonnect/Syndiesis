using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;
using Syndiesis.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Syndiesis.Controls;

public class DrillTransition : IPageTransition
{
    public TimeSpan Duration { get; set; }
    public Easing Easing { get; set; } = Singleton<ExponentialEaseOut>.Instance;
    public double ScaleRatio { get; set; } = 1.4;

    public async Task Start(
        Visual? from, Visual? to,
        bool forward, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        var commonParent = GetCommonVisualParent(from, to);
        if (commonParent is null)
        {
            throw new InvalidOperationException("Could not determine the common parent");
        }

        var scaleXProperty = ScaleTransform.ScaleXProperty;
        var scaleYProperty = ScaleTransform.ScaleYProperty;
        var opacityProperty = Visual.OpacityProperty;

        var duration = Duration;
        var easing = Easing;
        var ratio = ScaleRatio;
        var inverseRatio = 1 / ratio;

        var taskList = new List<Task>();

        if (from is not null)
        {
            var sourceOpacity = from.Opacity;

            var fromScale = 1D;
            var toScale = inverseRatio;
            if (!forward)
            {
                toScale = ratio;
            }

            var fadeOutAnimation = new Animation
            {
                Duration = duration,
                Easing = easing,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0.00),
                        Setters =
                        {
                            new Setter(scaleXProperty, fromScale),
                            new Setter(scaleYProperty, fromScale),
                            new Setter(opacityProperty, sourceOpacity),
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1.00),
                        Setters =
                        {
                            new Setter(scaleXProperty, toScale),
                            new Setter(scaleYProperty, toScale),
                            new Setter(opacityProperty, 0D),
                        }
                    },
                }
            };

            var fadeOutTransition = new TransitionAnimation(fadeOutAnimation);
            var task = fadeOutTransition.RunAsync(from, cancellationToken);
            taskList.Add(task);
        }

        if (to is not null)
        {
            var targetOpacity = 1D;

            var fromScale = ratio;
            var toScale = 1D;
            if (!forward)
            {
                fromScale = inverseRatio;
            }

            var fadeInAnimation = new Animation
            {
                Duration = duration,
                Easing = easing,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0.00),
                        Setters =
                        {
                            new Setter(scaleXProperty, fromScale),
                            new Setter(scaleYProperty, fromScale),
                            new Setter(opacityProperty, 0D),
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1.00),
                        Setters =
                        {
                            new Setter(scaleXProperty, toScale),
                            new Setter(scaleYProperty, toScale),
                            new Setter(opacityProperty, targetOpacity),
                        }
                    },
                }
            };

            var fadeInTransition = new TransitionAnimation(fadeInAnimation);
            var task = fadeInTransition.RunAsync(to, cancellationToken);
            taskList.Add(task);
        }

        await Task.WhenAll(taskList.ToArray());
    }

    private static Visual? GetCommonVisualParent(Visual? a, Visual? b)
    {
        var pa = a?.Parent as Visual;
        var pb = b?.Parent as Visual;

        if (pa != pb)
        {
            return null;
        }

        return pa;
    }
}
