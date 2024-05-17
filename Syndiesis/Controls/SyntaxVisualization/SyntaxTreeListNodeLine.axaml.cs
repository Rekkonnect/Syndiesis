using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Microsoft.CodeAnalysis.Text;
using Syndiesis.Controls.Inlines;
using Syndiesis.Controls.Toast;
using Syndiesis.Core.DisplayAnalysis;
using Syndiesis.Utilities;
using System;

namespace Syndiesis.Controls;

public partial class SyntaxTreeListNodeLine : UserControl
{
    private readonly CancellationTokenFactory _pulseLineCancellationTokenFactory = new();
    private readonly CancellationTokenFactory _pulseGroupedRunsCancellationTokenFactory = new();

    private GroupedRunInline? _hoveredRunInline;

    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<CodeEditorLine, bool>(nameof(IsExpanded), defaultValue: false);

    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set
        {
            SetValue(IsExpandedProperty, value);
            visualExpandToggle.IsExpandingToggle = !value;
        }
    }

    public static readonly StyledProperty<bool> HasChildrenProperty =
        AvaloniaProperty.Register<CodeEditorLine, bool>(nameof(HasChildren), defaultValue: true);

    public bool HasChildren
    {
        get => GetValue(HasChildrenProperty);
        set
        {
            SetValue(HasChildrenProperty, value);
            visualExpandToggle.IsVisible = value;
        }
    }

    public static readonly StyledProperty<string> NodeTypeTextProperty =
        AvaloniaProperty.Register<CodeEditorLine, string>(nameof(NodeTypeText), defaultValue: "N");

    public string NodeTypeText
    {
        get => GetValue(NodeTypeTextProperty!);
        set
        {
            SetValue(NodeTypeTextProperty!, value!);
            nodeTypeIconText.Text = value;
        }
    }

    public static readonly StyledProperty<Color> NodeTypeColorProperty =
        AvaloniaProperty.Register<CodeEditorLine, Color>(
            nameof(NodeTypeColor),
            defaultValue: NodeLineCreator.Styles.ClassMainColor);

    public Color NodeTypeColor
    {
        get => GetValue(NodeTypeColorProperty!);
        set
        {
            SetValue(NodeTypeColorProperty!, value!);
            nodeTypeIconText.Foreground = new SolidColorBrush(value);
        }
    }

    public NodeTypeDisplay NodeTypeDisplay
    {
        get
        {
            return new(NodeTypeText, NodeTypeColor);
        }
        set
        {
            var (text, color) = value;
            NodeTypeText = text;
            NodeTypeColor = color;
        }
    }

    public InlineCollection? Inlines
    {
        get => descriptionText.Inlines;
        set
        {
            descriptionText.Inlines!.ClearSetValues(value!);
        }
    }

    public GroupedRunInlineCollection? GroupedRunInlines
    {
        get => descriptionText.GroupedInlines;
        set
        {
            descriptionText.GroupedInlines = value;
        }
    }

    public SyntaxObjectInfo? AssociatedSyntaxObject { get; set; }

    public TextSpan DisplaySpan
    {
        get
        {
            var syntaxObject = AssociatedSyntaxObject;
            if (syntaxObject is null)
                return default;

            var nodeType = NodeTypeText;
            switch (nodeType)
            {
                case NodeLineCreator.Types.DisplayValue:
                    return syntaxObject.Span;
            }

            return syntaxObject.FullSpan;
        }
    }

    public LinePositionSpan DisplayLineSpan
    {
        get
        {
            var displaySpan = DisplaySpan;
            if (displaySpan == default)
                return default;

            var tree = AssociatedSyntaxObject!.SyntaxTree;
            return tree!.GetLineSpan(displaySpan).Span;
        }
    }

    public SyntaxTreeListNodeLine()
    {
        InitializeComponent();
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
                case KeyModifiers.Control:
                {
                    CopyEntireLineContent();
                    break;
                }

                case KeyModifiers.Control | KeyModifiers.Shift:
                {
                    CopyHoveredInlineContent();
                    break;
                }
            }
        }
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

    private void CopyEntireLineContent()
    {
        var text = descriptionText.Inlines!.Text;
        _ = this.SetClipboardTextAsync(text)
            .ConfigureAwait(false);
        PulseCopiedLine();

        var toastContainer = ToastNotificationContainer.GetFromMainWindowTopLevel(this);
        if (toastContainer is not null)
        {
            var popup = new ToastNotificationPopup();
            popup.defaultTextBlock.Text = $"""
                Copied entire line content:
                {text}
                """;
            var animation = new BlurOpenDropCloseToastAnimation(TimeSpan.FromSeconds(2));
            _ = toastContainer.Show(popup, animation);
        }
    }

    private void PulseCopiedLine()
    {
        _pulseLineCancellationTokenFactory.Cancel();
        var color = Color.FromArgb(192, 128, 128, 128);
        var animation = CreateColorPulseAnimation(this, color, BackgroundProperty);
        animation.Duration = TimeSpan.FromMilliseconds(750);
        animation.Easing = Singleton<CubicEaseOut>.Instance;
        _ = animation.RunAsync(this, _pulseLineCancellationTokenFactory.CurrentToken);
    }

    private void PulseCopiedTextInline()
    {
        _pulseGroupedRunsCancellationTokenFactory.Cancel();
        var color = Color.FromArgb(192, 128, 128, 128);
        var animation = CreateColorPulseAnimation(
            textPartHoverRectangle, color, Rectangle.FillProperty);
        animation.Duration = TimeSpan.FromMilliseconds(500);
        animation.Easing = Singleton<CubicEaseOut>.Instance;
        _ = animation.RunAsync(
            textPartHoverRectangle,
            _pulseGroupedRunsCancellationTokenFactory.CurrentToken);
    }

    private void DiscoverHoveredInlineEvaluate(PointerEventArgs e)
    {
        DiscoverHoveredInline(e);
        ReEvaluateKeyModifiers(e.KeyModifiers);
    }

    private void DiscoverHoveredInline(PointerEventArgs e)
    {
        var pointerPoint = e.GetCurrentPoint(descriptionText);
        var hoveredInline = descriptionText.HitTestGroupedRun(pointerPoint.Position);
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

            var bounds = descriptionText.RunBounds(hoveredInline)!.Value;
            var descriptionBounds = descriptionText.Bounds;
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

    private static Animation CreateColorPulseAnimation(
        Control control,
        Color fillColor,
        AvaloniaProperty<IBrush?> colorProperty)
    {
        return new()
        {
            Children =
            {
                new KeyFrame()
                {
                    Cue = new(0),
                    Setters =
                    {
                        new Setter()
                        {
                            Property = colorProperty,
                            Value = new SolidColorBrush(fillColor),
                        }
                    },
                },
                new KeyFrame()
                {
                    Cue = new(1),
                    Setters =
                    {
                        new Setter()
                        {
                            Property = colorProperty,
                            Value = control.GetValue(colorProperty),
                        }
                    },
                },
            }
        };
    }
}
