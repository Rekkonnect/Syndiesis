using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Microsoft.CodeAnalysis.Text;
using Syndiesis.Core.DisplayAnalysis;

namespace Syndiesis.Controls;

public partial class SyntaxTreeListNodeLine : UserControl
{
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
}

public readonly record struct NodeTypeDisplay(string Text, Color Color);
