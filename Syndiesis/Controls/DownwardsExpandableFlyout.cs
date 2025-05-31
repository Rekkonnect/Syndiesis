using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls.Primitives;

namespace Syndiesis.Controls;

public sealed class DownwardsExpandableFlyout : PopupFlyoutBase
{
    protected override Control CreatePresenter()
    {
        return new FlyoutPresenter
        {
            Transitions = new Transitions
            {
                new DoubleTransition
                {
                    Property = FlyoutPresenter.HeightProperty,
                    Duration = TimeSpan.FromMilliseconds(200),
                    Easing = new SineEaseIn(),
                },
            }
        };
    }
}
