using Avalonia.Animation;
using System.Threading.Tasks;

namespace CSharpSyntaxEditor.Utilities;

public class AnimationRunController(Animation animation, CancellationTokenFactory? tokenFactory = null)
{
    private readonly Animation _animation = animation;
    private readonly CancellationTokenFactory _tokenFactory = tokenFactory ?? new();

    public async Task RunAsync(Animatable animatable)
    {
        await _animation.RunAsync(animatable, _tokenFactory.CurrentToken);
    }

    public void Stop()
    {
        _tokenFactory.Cancel();
    }
}
