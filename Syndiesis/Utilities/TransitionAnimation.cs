using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Syndiesis.Utilities;

public sealed class TransitionAnimation(Animation animation)
{
    private readonly Animation _animation = animation;

    public async Task RunAsync(Animatable control, CancellationToken cancellationToken)
    {
        var animationTask = _animation.RunAsync(control, cancellationToken);
        ApplyFinalKeyFrame(control);

        await animationTask;

        if (cancellationToken.IsCancellationRequested)
        {
            ApplyFinalKeyFrame(control);
        }
    }

    private void ApplyFinalKeyFrame(Animatable control)
    {
        // set the final cue's setters -- which will persist after the animation
        var finalKeyFrame = _animation.Children.FirstOrDefault(s => s.Cue.CueValue is 1.0);
        if (finalKeyFrame is not null)
        {
            foreach (var setter in finalKeyFrame.Setters.OfType<Setter>())
            {
                setter.ApplySetter(control);
            }
        }
    }
}

public static class ControlExtensions
{
    public static void InvalidateAll(this Control control)
    {
        control.InvalidateArrange();
        control.InvalidateMeasure();
        control.InvalidateVisual();
    }

    public static void RecursivelyInvalidateAll(this Control control)
    {
        control.InvalidateAll();
        var parent = control.Parent;

        if (parent is Control parentControl)
        {
            parentControl.InvalidateAll();
        }
    }
}
