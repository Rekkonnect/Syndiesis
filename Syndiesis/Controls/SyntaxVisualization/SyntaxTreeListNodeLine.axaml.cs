using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Media;
using Syndiesis.Controls.SyntaxVisualization.Creation;
using Syndiesis.Utilities;

namespace Syndiesis.Controls;

[PseudoClasses(NodeLineHoverPseudoClass)]
public partial class SyntaxTreeListNodeLine : UserControl
{
    public const string NodeLineHoverPseudoClass = ":nodelinehover";

    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<CodeEditorLine, bool>(nameof(IsExpanded), defaultValue: true);

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

    public SyntaxObjectInfo? AssociatedSyntaxObject { get; set; }

    public SyntaxTreeListNodeLine()
    {
        InitializeComponent();
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        PseudoClasses.Set(NodeLineHoverPseudoClass, true);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        PseudoClasses.Set(NodeLineHoverPseudoClass, false);
    }
}

public readonly record struct NodeTypeDisplay(string Text, Color Color);
