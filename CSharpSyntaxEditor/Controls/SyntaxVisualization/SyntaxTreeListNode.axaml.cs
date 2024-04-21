using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using CSharpSyntaxEditor.Controls.SyntaxVisualization.Creation;
using CSharpSyntaxEditor.Utilities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CSharpSyntaxEditor.Controls;

[PseudoClasses(NodeHoverPseudoClass)]
public partial class SyntaxTreeListNode : UserControl
{
    public const string NodeHoverPseudoClass = ":nodehover";

    private static readonly CancellationTokenFactory _expansionAnimationCancellationTokenFactory = new();

    public object? AssociatedSyntaxObjectContent
    {
        get => AssociatedSyntaxObject?.SyntaxObject;
 set
        {
            var o = SyntaxObjectInfo.GetInfoForObject(value);
            AssociatedSyntaxObject = o;
        }
    }

    public SyntaxObjectInfo? AssociatedSyntaxObject
    {
        get => NodeLine.AssociatedSyntaxObject;
        set => NodeLine.AssociatedSyntaxObject = value;
    }

    public static readonly StyledProperty<SyntaxTreeListNodeLine> NodeLineProperty =
        AvaloniaProperty.Register<CodeEditorLine, SyntaxTreeListNodeLine>(
            nameof(NodeLine),
            defaultValue: new());

    public SyntaxTreeListNodeLine NodeLine
    {
        get => GetValue(NodeLineProperty);
        set
        {
            SetValue(NodeLineProperty, value);
            topNodeContent.Content = value;
            value.HasChildren = ChildNodes.Count > 0;
        }
    }

    public static readonly StyledProperty<AvaloniaList<SyntaxTreeListNode>> ChildNodesProperty =
        AvaloniaProperty.Register<CodeEditorLine, AvaloniaList<SyntaxTreeListNode>>(
            nameof(ChildNodes),
            defaultValue: []);

    public AvaloniaList<SyntaxTreeListNode> ChildNodes
    {
        get => GetValue(ChildNodesProperty);
        set
        {
            SetValue(ChildNodesProperty, value);
            innerStackPanel.Children.ClearSetValues(value);
            NodeLine.HasChildren = value.Count > 0;
        }
    }

    public SyntaxTreeListNode()
    {
        InitializeComponent();
    }

    private static readonly Color _topLineHoverColor = Color.FromArgb(64, 128, 128, 128);
    private static readonly Color _innerPanelHoverColor = Color.FromArgb(64, 96, 96, 96);

    private readonly SolidColorBrush _topLineBackgroundBrush = new(Colors.Transparent);
    private readonly SolidColorBrush _innerPanelBackgroundBrush = new(Colors.Transparent);

    private bool _isHovered;

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        EvaluateHovering(e);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        EvaluateHovering(e);
    }

    private void EvaluateHovering(PointerEventArgs e)
    {
        var nodeLine = NodeLine;
        var bounds = nodeLine.Bounds;
        var restrictedBounds = bounds.WithHeight(bounds.Height - 1);
        var isHovered = restrictedBounds.Contains(e.GetCurrentPoint(this).Position);

        if (isHovered == _isHovered)
            return;

        UpdateHovering(isHovered);
    }

    internal void UpdateHovering(bool isHovered)
    {
        _isHovered = isHovered;
        var topLineBackgroundColor = isHovered ? _topLineHoverColor : Colors.Transparent;
        var innerPanelBackgroundColor = isHovered ? _innerPanelHoverColor : Colors.Transparent;
        _topLineBackgroundBrush.Color = topLineBackgroundColor;
        _innerPanelBackgroundBrush.Color = innerPanelBackgroundColor;

        NodeLine.Background = _topLineBackgroundBrush;
        innerPanel.Background = _innerPanelBackgroundBrush;
    }

    internal void EvaluateHoveringRecursively(PointerEventArgs e)
    {
        foreach (var child in ChildNodes)
        {
            EvaluateHovering(e);
            child.EvaluateHoveringRecursively(e);
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        EvaluateHovering(e);
        if (_isHovered)
        {
            ToggleExpansion();
        }
    }

    public void ToggleExpansion()
    {
        var nodeLine = NodeLine;
        if (!nodeLine.HasChildren)
            return;

        bool newToggle = !nodeLine.IsExpanded;
        ExpandOrCollapse(newToggle);
    }

    public void Expand()
    {
        ExpandOrCollapse(true);
    }

    public void Collapse()
    {
        ExpandOrCollapse(false);
    }

    private void ExpandOrCollapse(bool expand)
    {
        var nodeLine = NodeLine;
        if (!nodeLine.HasChildren)
            return;

        if (nodeLine.IsExpanded == expand)
            return;

        nodeLine.IsExpanded = expand;

        // cancel any currently running animation
        _expansionAnimationCancellationTokenFactory.Cancel();

        var animationToken = _expansionAnimationCancellationTokenFactory.CurrentToken;
        _ = AnimateExpansionStackPanel(innerStackPanel, expand, animationToken);
    }

    private async Task AnimateExpansionStackPanel(
        StackPanel stackPanel,
        bool expand,
        CancellationToken cancellationToken)
    {
        var totalHeight = stackPanel.Children.Sum(c => c.Bounds.Height);
        var from = expand ? 0 : totalHeight;
        var to = expand ? totalHeight : 0;

        double startOpacity = expand ? 0.0 : 1.0;
        double targetOpacity = expand ? 1.0 : 0.0;

        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(300),
            Easing = new CubicEaseOut(),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0.00),
                    Setters =
                    {
                        new Setter(StackPanel.HeightProperty, from),
                        new Setter(StackPanel.OpacityProperty, startOpacity),
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1.00),
                    Setters =
                    {
                        new Setter(StackPanel.HeightProperty, to),
                        new Setter(StackPanel.OpacityProperty, targetOpacity),
                    }
                },
            }
        };

        await new TransitionAnimation(animation)
            .RunAsync(stackPanel, cancellationToken);
    }
}
