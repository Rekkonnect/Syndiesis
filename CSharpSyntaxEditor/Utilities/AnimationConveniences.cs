using Avalonia.Animation;

namespace CSharpSyntaxEditor.Utilities;

public static class AnimationConveniences
{
    /// <summary>
    /// Provides a practically infinite iteration count, to allow running the
    /// animation with <see cref="Animation.RunAsync(Animatable, System.Threading.CancellationToken)"/>
    /// without throwing an exception.
    /// </summary>
    public static readonly IterationCount NearInfiniteIterationCount
        = new(ulong.MaxValue);
}
