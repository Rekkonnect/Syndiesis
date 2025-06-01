using Avalonia.Animation.Easings;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Garyon.Exceptions;
using Garyon.Objects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Syndiesis.Controls.Inlines;
using Syndiesis.Controls.Toast;
using Syndiesis.Core.DisplayAnalysis;
using Syndiesis.Utilities;

namespace Syndiesis.Controls.AnalysisVisualization;

public partial class AnalysisTreeListNodeLine : UserControl
{
    private readonly CancellationTokenFactory _pulseLineCancellationTokenFactory = new();

    private bool _isExpanded;

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            _isExpanded = value;
            visualExpandToggle.IsExpandingToggle = !value;
        }
    }

    private bool _hasChildren;

    public bool HasChildren
    {
        get => _hasChildren;
        set
        {
            _hasChildren = value;
            visualExpandToggle.IsVisible = value;
        }
    }

    private string _nodeTypeText = string.Empty;

    public string NodeTypeText
    {
        get => _nodeTypeText;
        set
        {
            _nodeTypeText = value;
            nodeTypeIconText.Text = value;
        }
    }

    // TODO: Remove this
    public static readonly StyledProperty<Color> NodeTypeColorProperty =
        AvaloniaProperty.Register<AnalysisTreeListNode, Color>(
            nameof(NodeTypeColor),
            defaultValue: BaseAnalysisNodeCreator.CommonStyles.ClassMainColor);

    public Color NodeTypeColor
    {
        get => GetValue(NodeTypeColorProperty!);
        set
        {
            SetValue(NodeTypeColorProperty!, value!);
            // TODO: Avoid creating a new brush for every node
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

    public TextSpanSource DisplaySpanSource { get; set; } = TextSpanSource.FullSpan;

    public TextSpan DisplaySpan
    {
        get
        {
            var syntaxObject = AssociatedSyntaxObject;
            if (syntaxObject is null)
                return default;

            switch (DisplaySpanSource)
            {
                case TextSpanSource.Span:
                    return syntaxObject.Span;
                case TextSpanSource.FullSpan:
                    return syntaxObject.FullSpan;

                default:
                    ThrowHelper<UnreachableException>.Throw();
                    // unreachable
                    return default;
            }
        }
    }

    public AnalysisNodeKind AnalysisNodeKind { get; set; }

    private AnalysisNodeLineContentState _contentState;

    public AnalysisNodeLineContentState ContentState
    {
        get
        {
            return _contentState;
        }
        set
        {
            if (_contentState == value)
                return;

            _contentState = value;

            bool isLoading = value is AnalysisNodeLineContentState.Loading;
            if (isLoading)
            {
                optionalLoadingNodeContainer.Content ??= new LoadingSpinner();
            }
            optionalLoadingNodeContainer.IsVisible = isLoading;

            bool isFailed = value is AnalysisNodeLineContentState.Failed;
            if (isFailed)
            {
                errorNodeContainer.Content ??=
                    App.CurrentResourceManager.FailureImage?.CopyOfSource();
            }
            errorNodeContainer.IsVisible = isFailed;

            nodeTypeIconText.IsVisible = value is AnalysisNodeLineContentState.Loaded;
        }
    }

    public AnalysisTreeListNodeLine()
    {
        InitializeComponent();
    }

    public LinePositionSpan DisplayLineSpan(SyntaxTree? tree)
    {
        if (tree is null)
            return default;

        var displaySpan = DisplaySpan;
        if (displaySpan == default)
            return default;

        return tree!.GetLineSpan(displaySpan).Span;
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

        var toastContainer = ToastNotificationContainer.GetFromOuterMainViewContainer(this);
        _ = CommonToastNotifications.ShowClassicMain(
            toastContainer,
            $"""
            Copied entire line content:
            {text}
            """,
            TimeSpan.FromSeconds(2));
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

    public void ReloadFromBuilder(UIBuilder.AnalysisTreeListNodeLine builder)
    {
        builder.BuildOnto(this);
    }
}
