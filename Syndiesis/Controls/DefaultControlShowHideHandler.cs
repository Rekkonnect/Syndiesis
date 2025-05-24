using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Styling;
using Garyon.Objects;
using System;

namespace Syndiesis.Controls;

public class DefaultControlShowHideHandler
    : ControlShowHideHandler
{
    public override async Task Show(Control? control)
    {
        if (control is IShowHideControl showHide)
        {
            await showHide.Show();
            return;
        }

        await ShowDefault(control);
    }

    public override async Task Hide(Control? control)
    {
        if (control is IShowHideControl showHide)
        {
            await showHide.Hide();
            return;
        }

        await HideDefault(control);
    }

    private async Task ShowDefault(Control? control)
    {
        if (control is null)
        {
            return;
        }

        var opacityProperty = Visual.OpacityProperty;
        var marginProperty = Layoutable.MarginProperty;

        var targetMargin = control.Margin;
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(350),
            Easing = Singleton<ExponentialEaseOut>.Instance,
            Children =
            {
                new KeyFrame
                {
                    Cue = new(0),
                    Setters =
                    {
                        new Setter
                        {
                            Property = marginProperty,
                            Value = targetMargin.WithBottom(targetMargin.Bottom - 20),
                        },
                        new Setter
                        {
                            Property = opacityProperty,
                            Value = 0D,
                        },
                    }
                },
                new KeyFrame
                {
                    Cue = new(1),
                    Setters =
                    {
                        new Setter
                        {
                            Property = marginProperty,
                            Value = targetMargin,
                        },
                        new Setter
                        {
                            Property = opacityProperty,
                            Value = 1D,
                        },
                    }
                },
            },
        };
        var transitionAnimation = new TransitionAnimation(animation);
        await transitionAnimation.RunAsync(control, default);
    }

    private async Task HideDefault(Control? control)
    {
        if (control is null)
        {
            return;
        }

        var opacityProperty = Visual.OpacityProperty;
        var marginProperty = Layoutable.MarginProperty;

        var targetMargin = control.Margin;
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(350),
            Easing = Singleton<ExponentialEaseOut>.Instance,
            Children =
            {
                new KeyFrame
                {
                    Cue = new(0),
                    Setters =
                    {
                        new Setter
                        {
                            Property = marginProperty,
                            Value = targetMargin,
                        },
                        new Setter
                        {
                            Property = opacityProperty,
                            Value = 1D,
                        },
                    }
                },
                new KeyFrame
                {
                    Cue = new(1),
                    Setters =
                    {
                        new Setter
                        {
                            Property = marginProperty,
                            Value = targetMargin.WithBottom(targetMargin.Bottom - 20),
                        },
                        new Setter
                        {
                            Property = opacityProperty,
                            Value = 0D,
                        },
                    }
                },
            },
        };
        var transitionAnimation = new TransitionAnimation(animation);
        await transitionAnimation.RunAsync(control, default);
    }
}
