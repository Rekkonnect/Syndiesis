using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;
using CSharpSyntaxEditor.Utilities;
using System;
using System.Threading.Tasks;

namespace CSharpSyntaxEditor.Controls;

public partial class CoverableContent : UserControl
{
    private readonly CancellationTokenFactory _animationCancellationFactory = new();

    public object? ContainedContent
    {
        get => content.Content;
        set => content.Content = value;
    }

    public CoverableContent()
    {
        InitializeComponent();
    }

    public async Task HideCover(TimeSpan animationDuration)
    {
        await AnimateWithTargetOpacity(0, animationDuration);
    }

    public async Task ShowCover(Control? content, string text, TimeSpan animationDuration)
    {
        cover.IconDisplay = content;
        cover.DisplayText = text;
        await AnimateWithTargetOpacity(1, animationDuration);
    }

    private async Task AnimateWithTargetOpacity(
        double targetOpacity,
        TimeSpan duration)
    {
        var currentOpacity = cover.Opacity;
        if (currentOpacity == targetOpacity)
            return;

        outerPanel.Children.AddIfNotContained(cover);

        var animation = new Animation
        {
            Duration = duration,
            Children =
            {
                new KeyFrame
                {
                    Cue = new(0),
                    Setters =
                    {
                        new Setter(OpacityProperty, currentOpacity)
                    }
                },
                new KeyFrame
                {
                    Cue = new(1),
                    Setters =
                    {
                        new Setter(OpacityProperty, targetOpacity)
                    }
                },
            },
        };

        var transition = new TransitionAnimation(animation);
        _animationCancellationFactory.Cancel();
        var currentToken = _animationCancellationFactory.CurrentToken;
        await transition.RunAsync(cover, currentToken);

        // do not remove the child if the operation has been cancelled beforehand
        currentToken.ThrowIfCancellationRequested();

        // required to avoid having the control cover the entire
        if (targetOpacity is 0)
        {
            outerPanel.Children.Remove(cover);
        }
    }
}
