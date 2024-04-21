using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Media;
using CSharpSyntaxEditor.Controls.Extensions;
using CSharpSyntaxEditor.Controls.SyntaxVisualization.Creation;
using CSharpSyntaxEditor.Utilities;
using System.Collections.Generic;

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
        expandableCanvas.SetExpansionStateWithoutAnimation(ExpansionState.Expanded);
    }

    private static readonly Color _topLineHoverColor = Color.FromArgb(64, 128, 128, 128);
    private static readonly Color _expandableCanvasHoverColor = Color.FromArgb(64, 96, 96, 96);

    private readonly SolidColorBrush _topLineBackgroundBrush = new(Colors.Transparent);
    private readonly SolidColorBrush _expandableCanvasBackgroundBrush = new(Colors.Transparent);

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
        var expandableCanvasBackgroundColor = isHovered ? _expandableCanvasHoverColor : Colors.Transparent;
        _topLineBackgroundBrush.Color = topLineBackgroundColor;
        _expandableCanvasBackgroundBrush.Color = expandableCanvasBackgroundColor;

        NodeLine.Background = _topLineBackgroundBrush;
        expandableCanvas.Background = _expandableCanvasBackgroundBrush;
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

    internal void CorrectContainedNodeWidths(Size availableSize)
    {
        var leftMargin = innerStackPanel.Margin.Left;
        var nextWidth = availableSize.Width - leftMargin;
        var nextAvailableSize = availableSize.WithWidth(nextWidth);

        Width = availableSize.Width;

        foreach (var child in ChildNodes)
        {
            child.CorrectContainedNodeWidths(nextAvailableSize);
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
        _ = this.expandableCanvas.SetExpansionState(expand, animationToken);
    }

    public IEnumerable<SyntaxTreeListNode> EnumerateNodes()
    {
        yield return this;

        foreach (var child in ChildNodes)
        {
            var enumeratedNodes = child.EnumerateNodes();
            foreach (var node in enumeratedNodes)
            {
                yield return node;
            }
        }
    }
}
