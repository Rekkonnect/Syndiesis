using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Styling;
using CSharpSyntaxEditor.Utilities;
using System;
using System.Threading.Tasks;

namespace CSharpSyntaxEditor.Controls;

public partial class CoverableContent : UserControl
{
    /*
     * The intend of this class is to allow animating the cover on top
     * of the control that is being displayed. The cover will be dynamically
     * added and removed from the panel containing both the cover and the
     * control below the cover. Once the cover is completely transparent, it is
     * removed from the panel.
     * 
     * We allow cancelling an active cover task in favor of another cover task,
     * as long as the effects are opposite. The opacity animation respects the
     * current value of the opacity of the cover, therefore theoretically allowing
     * smooth transitions after interrupting the active cover task.
     * 
     * The cancellation token factory provides a common source of cancellation tokens
     * for both animation sources, the hiding and the showing of the cover. Once the
     * animation is ready to begin, the active cover animation is cancelled, followed
     * by immediately beginning the newly set animation.
     * 
     * If another show animation is requested while there is a current active show
     * animation, the second one will not take place, and the first one will keep
     * running. Not doing so will interrupt previous animations and introduce another
     * delay between the current animation and the interrupting one.
     * The same applies for hide animations.
     */

    private readonly CancellationTokenFactory _animationCancellationFactory = new();

    private Task? _showCoverTask;
    private Task? _hideCoverTask;

    private readonly object _taskRunningUpdateLock = new();
    private volatile bool _isUpdatingRunningTasks = false;

    public object? ContainedContent
    {
        get => content.Content;
        set => content.Content = value;
    }

    public CoverableContent()
    {
        InitializeComponent();
    }

    private void UpdateRunningTasks()
    {
        if (_isUpdatingRunningTasks)
            return;

        lock (_taskRunningUpdateLock)
        {
            if (_isUpdatingRunningTasks)
                return;

            _isUpdatingRunningTasks = true;
            FreeCompletedTask(ref _showCoverTask);
            FreeCompletedTask(ref _hideCoverTask);
            _isUpdatingRunningTasks = false;
        }
    }

    private static void FreeCompletedTask(ref Task? taskField)
    {
        if (taskField?.IsCompleted is true)
        {
            taskField = null;
        }
    }

    public void UpdateCoverContent(Control? content, string text)
    {
        cover.IconDisplay = content;
        cover.DisplayText = text;
    }

    public async Task CancelCurrentTaskAnimation()
    {
        _animationCancellationFactory.Cancel();
        if (_showCoverTask is not null)
        {
            await _showCoverTask;
        }
        if (_hideCoverTask is not null)
        {
            await _hideCoverTask;
        }

        UpdateRunningTasks();
    }

    public async Task HideCover(TimeSpan animationDuration)
    {
        UpdateRunningTasks();
        if (_hideCoverTask is not null)
            return;

        _hideCoverTask = AnimateWithTargetOpacity(0, animationDuration);
        await _hideCoverTask;
    }

    public async Task ShowCover(Control? content, string text, TimeSpan animationDuration)
    {
        UpdateCoverContent(content, text);

        UpdateRunningTasks();
        if (_showCoverTask is not null)
            return;

        _showCoverTask = AnimateWithTargetOpacity(1, animationDuration);
        await _showCoverTask;
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
            Easing = new ExponentialEaseInOut(),
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
        await CancelCurrentTaskAnimation();
        var currentToken = _animationCancellationFactory.CurrentToken;
        await transition.RunAsync(cover, currentToken);

        // do not remove the child if the operation has been cancelled beforehand
        if (currentToken.IsCancellationRequested)
            return;

        // required to avoid having the control cover the entire
        if (targetOpacity is 0)
        {
            outerPanel.Children.Remove(cover);
        }
    }
}
