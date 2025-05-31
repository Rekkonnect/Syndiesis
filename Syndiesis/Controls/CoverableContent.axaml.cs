using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Garyon.Objects;

namespace Syndiesis.Controls;

public partial class CoverableContent : UserControl
{
    private readonly DoubleTransition _opacityTransition = new()
    {
        Property = OpacityProperty,
        Duration = TimeSpan.FromMilliseconds(350),
        Easing = Singleton<ExponentialEaseInOut>.Instance,
    };

    public object? ContainedContent
    {
        get => content.Content;
        set => content.Content = value;
    }

    public CoverableContent()
    {
        InitializeComponent();
        InitializeTransitions();
    }

    private void InitializeTransitions()
    {
        cover.Transitions = [_opacityTransition];
    }

    public void UpdateCoverContent(Control? content, string text, IBrush? textBrush = null)
    {
        cover.IconDisplay = content;
        cover.DisplayText = text;
        textBrush ??= UserInteractionCover.Styling.GoodTextBrush;
        cover.TextBrush = textBrush;
    }

    public void HideCover(TimeSpan animationDuration)
    {
        _opacityTransition.Duration = animationDuration;
        cover.Opacity = 0;
        cover.IsHitTestVisible = false;
    }

    public void ShowCover(Control? content, string text, TimeSpan animationDuration)
    {
        UpdateCoverContent(content, text);
        _opacityTransition.Duration = animationDuration;
        cover.Opacity = 1;
        cover.IsHitTestVisible = true;
    }
}
