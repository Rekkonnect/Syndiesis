using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Garyon.Objects;
using Syndiesis.Controls.Toast;
using Syndiesis.Utilities;
using System;

namespace Syndiesis.Controls.Inlines;

public partial class CopyableGroupedRunInlineTextBlock : UserControl
{
    private readonly CancellationTokenFactory _pulseGroupedRunsCancellationTokenFactory = new();

    private GroupedRunInline? _hoveredRunInline;

    private volatile bool _redrawn;
    private PointerEventArgs? _lastPointerEventArgs;

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

    public string Text
    {
        set
        {
            SetWholeCopyableText(value);
        }
    }

    public TextWrapping TextWrapping
    {
        get => containedText.TextWrapping;
        set => containedText.TextWrapping = value;
    }

    public CopyableGroupedRunInlineTextBlock()
    {
        InitializeComponent();
        ClipToBounds = false;
        containedText.LayoutUpdated += OnContainedTextLayoutUpdated;
    }

    public void SetWholeCopyableText(string text)
    {
        GroupedRunInlines = new GroupedRunInlineCollection
        {
            new SingleRunInline(new Run(text)
            {
                FontFamily = FontFamily,
                FontSize = FontSize,
            }),
        };
    }

    private void OnContainedTextLayoutUpdated(object? sender, EventArgs e)
    {
        if (_redrawn)
        {
            DiscoverHoveredInlineEvaluate();
            _redrawn = false;
        }
    }

    public void Redraw()
    {
        containedText.InvalidateText();
        _redrawn = true;
    }

    private void HandleRootKeyEvent(object? sender, KeyEventArgs e)
    {
        ReEvaluateKeyModifiers(e.KeyModifiers);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        _lastPointerEventArgs = e;
        var root = VisualRoot as InputElement;
        if (root is null)
            return;
        root!.KeyDown += HandleRootKeyEvent;
        root!.KeyUp += HandleRootKeyEvent;
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        _lastPointerEventArgs = e;
        ClearHoveredInline();

        var root = VisualRoot as InputElement;
        if (root is null)
            return;
        root.KeyDown -= HandleRootKeyEvent;
        root.KeyUp -= HandleRootKeyEvent;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        _lastPointerEventArgs = e;
        DiscoverHoveredInlineEvaluate(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        _lastPointerEventArgs = e;
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

    private void DiscoverHoveredInlineEvaluate()
    {
        if (_lastPointerEventArgs is not null)
        {
            DiscoverHoveredInlineEvaluate(_lastPointerEventArgs);
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
        if (_hoveredRunInline == hoveredInline && !_redrawn)
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
        _ = CommonToastNotifications.ShowClassicMain(
            toastContainer,
            $"""
             Copied partial line content:
             {text}
             """,
            TimeSpan.FromSeconds(2));
    }
}
