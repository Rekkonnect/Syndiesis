using Avalonia.Animation;
using Garyon.Objects;

namespace Syndiesis.Controls;

public sealed class ReusableCancellableAnimation(Animation animation)
{
    public readonly Animation Animation = animation;
    public readonly CancellationTokenFactory CancellationTokenFactory = new();

    public async Task RunAsync(Animatable animatable)
    {
        CancellationTokenFactory.Cancel();
        await Animation.RunAsync(animatable, CancellationTokenFactory.CurrentToken);
    }
}
