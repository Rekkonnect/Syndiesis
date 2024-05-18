using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Syndiesis.Controls.Toast;
using Syndiesis.Utilities;
using System;

namespace Syndiesis.Controls.Inlines;

public partial class CopyableGroupedRunInlineTextBlock : UserControl
{
    private readonly CancellationTokenFactory _pulseGroupedRunsCancellationTokenFactory = new();

    private GroupedRunInline? _hoveredRunInline;

    public InlineCollection? Inlines
    {
        get => containedText.Inlines;
        set
        {
            containedText.Inlines!.ClearSetValues(value!);
        }
    }

    public GroupedRunInlineCollection? GroupedRunInlines
    {
        get => containedText.GroupedInlines;
        set
        {
            containedText.GroupedInlines = value;
        }
    }

    public CopyableGroupedRunInlineTextBlock()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        var root = VisualRoot as InputElement;
        root!.KeyDown += HandleRootKeyEvent;
        root!.KeyUp += HandleRootKeyEvent;
    }

    private void HandleRootKeyEvent(object? sender, KeyEventArgs e)
    {
        ReEvaluateKeyModifiers(e.KeyModifiers);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        ClearHoveredInline();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        DiscoverHoveredInlineEvaluate(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        DiscoverHoveredInlineEvaluate(e);

        var pointerPoint = e.GetCurrentPoint(this);
        var properties = pointerPoint.Properties;
        if (properties.IsLeftButtonPressed)
        {
            var modifiers = e.KeyModifiers.NormalizeByPlatform();
            switch (modifiers)
            {
                case KeyModifiers.Control | KeyModifiers.Shift:
                {
                    CopyHoveredInlineContent();
                    break;
                }
            }
        }
    }

    private void DiscoverHoveredInlineEvaluate(PointerEventArgs e)
    {
        DiscoverHoveredInline(e);
        ReEvaluateKeyModifiers(e.KeyModifiers);
    }

    private void DiscoverHoveredInline(PointerEventArgs e)
    {
        var pointerPoint = e.GetCurrentPoint(containedText);
        var hoveredInline = containedText.HitTestGroupedRun(pointerPoint.Position);
        SetBackgroundHoverForInline(hoveredInline);
    }

    private void ClearHoveredInline()
    {
        SetBackgroundHoverForInline(null);
    }

    public void ReEvaluateKeyModifiers(KeyModifiers modifiers)
    {
        var canCopy = CanCopyPartialTextBlock(modifiers);
        textPartHoverRectangle.IsVisible = canCopy && _hoveredRunInline is not null;
    }

    private void SetBackgroundHoverForInline(GroupedRunInline? hoveredInline)
    {
        textPartHoverRectangle.IsVisible = hoveredInline is not null;
        UpdateHoveredInline(hoveredInline);
    }

    private void UpdateHoveredInline(GroupedRunInline? hoveredInline)
    {
        if (_hoveredRunInline == hoveredInline)
            return;

        _hoveredRunInline = hoveredInline;

        if (hoveredInline is not null)
        {
            const double extraWidth = 0.7;
            const double extraHeight = 0;

            var bounds = containedText.RunBounds(hoveredInline)!.Value;
            var descriptionBounds = containedText.Bounds;
            Canvas.SetLeft(textPartHoverRectangle, bounds.Left - extraWidth + descriptionBounds.Left);
            Canvas.SetTop(textPartHoverRectangle, bounds.Top - extraHeight + descriptionBounds.Top);
            textPartHoverRectangle.Width = bounds.Width + 2 * extraWidth;
            textPartHoverRectangle.Height = bounds.Height + 2 * extraHeight;
            _pulseGroupedRunsCancellationTokenFactory.Cancel();
        }
    }

    private static bool CanCopyPartialTextBlock(KeyModifiers modifiers)
    {
        return modifiers.NormalizeByPlatform()
            is (KeyModifiers.Control | KeyModifiers.Shift);
    }

    private void PulseCopiedTextInline()
    {
        _pulseGroupedRunsCancellationTokenFactory.Cancel();
        var color = Color.FromArgb(192, 128, 128, 128);
        var animation = Animations.CreateColorPulseAnimation(
            textPartHoverRectangle, color, Rectangle.FillProperty);
        animation.Duration = TimeSpan.FromMilliseconds(500);
        animation.Easing = Singleton<CubicEaseOut>.Instance;
        _ = animation.RunAsync(
            textPartHoverRectangle,
            _pulseGroupedRunsCancellationTokenFactory.CurrentToken);
    }

    private void CopyHoveredInlineContent()
    {
        if (_hoveredRunInline is null)
            return;

        var text = _hoveredRunInline.EffectiveText();
        _ = this.SetClipboardTextAsync(text)
            .ConfigureAwait(false);
        PulseCopiedTextInline();

        var toastContainer = ToastNotificationContainer.GetFromMainWindowTopLevel(this);
        if (toastContainer is not null)
        {
            var popup = new ToastNotificationPopup();
            popup.defaultTextBlock.Text = $"""
                Copied partial line content:
                {text}
                """;
            var animation = new BlurOpenDropCloseToastAnimation(TimeSpan.FromSeconds(2));
            _ = toastContainer.Show(popup, animation);
        }
    }
}
