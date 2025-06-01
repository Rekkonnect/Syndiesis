using Avalonia.Animation;
using Avalonia.Styling;

namespace Syndiesis.Controls;

public sealed class TransitionAnimation(Animation animation)
{
    private readonly Animation _animation = animation;

    public async Task RunAsync(Animatable control, CancellationToken cancellationToken)
    {
        var animationTask = _animation.RunAsync(control, cancellationToken);
        ApplyFinalKeyFrame(control);

        await animationTask;

        // Sometimes the animation might not finish its final frame correctly, if frames
        // get skipped due to lag, so we set the final key frame again
        ApplyFinalKeyFrame(control);
    }

    private void ApplyFinalKeyFrame(Animatable control)
    {
        var keyframes = _animation.Children;
        if (keyframes is [])
            return;

        // set the final cue's setters -- which will persist after the animation
        var finalKeyFrame = _animation.Children.MaxBy(s => s.Cue.CueValue)!;
        foreach (var setter in finalKeyFrame.Setters.OfType<Setter>())
        {
            setter.ApplySetter(control);
        }
    }
}
