using Avalonia.Animation;
using Garyon.Objects;

namespace Syndiesis.Controls;

/// <summary>
/// This provides functionality similar to <see cref="TransitioningContentControl"/>
/// but without removing the children that are transitioned between.
/// Works best for two controls transitioning with each other.
/// </summary>
/// <remarks>
/// Why this was needed:
/// Transitioning back to the main window which includes the AvaloniaEdit control would
/// cause a very long freeze (500ms or so) for about 500 lines of code, which is due to
/// re-rendering the entire text from the measure invalidation. This invalidation was a
/// result of the automatic removal and re-inclusion of the control into the underlying
/// content presenters of <see cref="TransitioningContentControl"/>. This control mitigates
/// that by keeping the controls rendered and only transitioning between them, which
/// transition also handles the opacity of the controls.
/// </remarks>
public partial class TransitioningMainPage : UserControl
{
    public static readonly StyledProperty<IPageTransition> PageTransitionProperty =
        AvaloniaProperty.Register<TransitioningMainPage, IPageTransition>(nameof(PageTransition));

    public IPageTransition PageTransition
    {
        get => GetValue(PageTransitionProperty);
        set => SetValue(PageTransitionProperty, value);
    }

    private ContentIndex _contentIndex;

    private readonly CancellationTokenFactory _transitionCancellationTokenFactory = new();

    public bool IsTransitioningForward { get; set; } = true;
    public bool AutoFocusOnTransition { get; set; } = true;

    public TransitioningMainPage()
    {
        InitializeComponent();
    }

    public void SetMainContent(Control mainContent)
    {
        MainContent.Children.ClearSetValue(mainContent);
    }

    public void SetSecondaryContent(Control transitionContent)
    {
        SecondaryContent.Children.ClearSetValue(transitionContent);
    }

    public void TransitionToSecondary(Control? transition)
    {
        if (transition is not null)
        {
            SetSecondaryContent(transition);
        }

        TransitionToSecondary();
    }

    public void TransitionToMain()
    {
        PerformTransition(
            SecondaryContent,
            MainContent,
            ContentIndex.Main,
            false);
    }

    public void TransitionToSecondary()
    {
        PerformTransition(
            MainContent,
            SecondaryContent,
            ContentIndex.Secondary,
            true);
    }

    private void PerformTransition(
        Panel from,
        Panel to,
        ContentIndex targetIndex,
        bool forward)
    {
        if (_contentIndex == targetIndex)
            return;

        _contentIndex = targetIndex;

        _transitionCancellationTokenFactory.Cancel();
        Dispatcher.UIThread.InvokeAsync(() =>
            PageTransition.Start(
                from,
                to,
                forward == IsTransitioningForward,
                _transitionCancellationTokenFactory.CurrentToken));
        from.IsHitTestVisible = false;
        to.IsHitTestVisible = true;

        if (AutoFocusOnTransition)
        {
            to.Children.First().Focus();
        }
    }

    private enum ContentIndex
    {
        Main,
        Secondary,
    }
}
