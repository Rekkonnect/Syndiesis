using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Styling;
using Syndiesis.Core;
using Syndiesis.Utilities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Syndiesis.Controls.Extensions;

public class VerticallyExpandablePanel : Panel
{
    public ExpansionState ExpansionState { get; private set; } = ExpansionState.Collapsed;

    public static readonly StyledProperty<double> ChildrenHeightProperty =
        AvaloniaProperty.Register<VerticallyExpandablePanel, double>(
            nameof(ChildrenHeight),
            defaultBindingMode: BindingMode.OneWay);

    public double ChildrenHeight
    {
        get
        {
            return Children.Sum(c => c.Height);
        }
    }

    public static readonly DirectProperty<VerticallyExpandablePanel, double>
        ChildrenHeightRatioProperty =
            AvaloniaProperty.RegisterDirect<VerticallyExpandablePanel, double>(
                nameof(ChildrenHeightRatio),
                p => p.ChildrenHeightRatio,
                (p, value) => p.ChildrenHeightRatio = value);

    private double _childrenHeightRatio = 0;

    public double ChildrenHeightRatio
    {
        get
        {
            return _childrenHeightRatio;
        }
        set
        {
            _childrenHeightRatio = value;
            Height = ChildrenHeight * value;
            InvalidateMeasure();
        }
    }

    public double ChildrenDesiredHeight
    {
        get
        {
            return Children.Sum(c => c.DesiredSize.Height);
        }
    }

    public double ChildrenBoundsHeight
    {
        get
        {
            return Children.Sum(c => c.Bounds.Height);
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var totalSize = Size.Infinity;
        foreach (var child in Children)
        {
            child.Measure(totalSize);
            var childMeasurement = child.DesiredSize;
            totalSize = totalSize.Constrain(childMeasurement);
        }
        var height = _childrenHeightRatio * totalSize.Height;
        totalSize = totalSize
            .WithHeight(height)
            ;

        return totalSize;
    }

    public void SetExpansionStateWithoutAnimation(ExpansionState state)
    {
        bool expand = state is ExpansionState.Expanded;

        ExpansionState = state;
        ChildrenHeightRatio = expand ? 1.0 : 0.0;
        Opacity = expand ? 1.0 : 0.0;
    }

    public async Task Collapse(CancellationToken cancellationToken)
    {
        await SetExpansionState(ExpansionState.Collapsed, cancellationToken);
    }

    public async Task Expand(CancellationToken cancellationToken)
    {
        await SetExpansionState(ExpansionState.Expanded, cancellationToken);
    }

    public async Task SetExpansionState(bool expand, CancellationToken cancellationToken)
    {
        var state = expand ? ExpansionState.Expanded : ExpansionState.Collapsed;
        await SetExpansionState(state, cancellationToken);
    }

    public async Task SetExpansionState(ExpansionState state, CancellationToken cancellationToken)
    {
        if (ExpansionState == state)
            return;

        ExpansionState = state;
        await AnimateExpansion(state is ExpansionState.Expanded, cancellationToken);
    }

    private async Task AnimateExpansion(
        bool expand,
        CancellationToken cancellationToken)
    {
        double from = expand ? 0.0 : 1.0;
        double to = expand ? 1.0 : 0.0;

        double startOpacity = expand ? 0.0 : 1.0;
        double targetOpacity = expand ? 1.0 : 0.0;

        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(300),
            Easing = Singleton<CubicEaseOut>.Instance,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0.00),
                    Setters =
                    {
                        new Setter(ChildrenHeightRatioProperty, from),
                        new Setter(OpacityProperty, startOpacity),
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1.00),
                    Setters =
                    {
                        new Setter(ChildrenHeightRatioProperty, to),
                        new Setter(OpacityProperty, targetOpacity),
                    }
                },
            }
        };

        await new TransitionAnimation(animation)
            .RunAsync(this, cancellationToken);
    }

    public async Task AnimateCurrentHeight(
        CancellationToken cancellationToken)
    {
        double from = MaxHeight;
        double to = ChildrenBoundsHeight;

        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(300),
            Easing = Singleton<CubicEaseOut>.Instance,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0.00),
                    Setters =
                    {
                        new Setter(MaxHeightProperty, from),
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1.00),
                    Setters =
                    {
                        new Setter(MaxHeightProperty, to),
                    }
                },
            }
        };

        await new TransitionAnimation(animation)
            .RunAsync(this, cancellationToken);
    }
}
