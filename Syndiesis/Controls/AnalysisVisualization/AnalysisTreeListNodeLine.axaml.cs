using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Media;
using Microsoft.CodeAnalysis.Text;
using Syndiesis.Controls;
using Syndiesis.Controls.Inlines;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Toast;
using Syndiesis.Core.DisplayAnalysis;
using Syndiesis.Utilities;
using Syndiesis.Views;
using System;

namespace Syndiesis.Controls.AnalysisVisualization;

public partial class AnalysisTreeListNodeLine : UserControl
{
    private readonly CancellationTokenFactory _pulseLineCancellationTokenFactory = new();

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
            defaultValue: SyntaxAnalysisNodeCreator.Styles.ClassMainColor);

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
        get => descriptionText.GroupedRunInlines;
        set
        {
            descriptionText.GroupedRunInlines = value;
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
                case SyntaxAnalysisNodeCreator.Types.DisplayValue:
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

    public AnalysisTreeListNodeLine()
    {
        InitializeComponent();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
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
            }
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
        var animation = Animations.CreateColorPulseAnimation(this, color, BackgroundProperty);
        animation.Duration = TimeSpan.FromMilliseconds(750);
        animation.Easing = Singleton<CubicEaseOut>.Instance;
        _ = animation.RunAsync(this, _pulseLineCancellationTokenFactory.CurrentToken);
    }
}
