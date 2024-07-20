using Avalonia.Controls.Documents;
using Avalonia.Media;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Inlines;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;

namespace Syndiesis.Core.DisplayAnalysis;

using ARun = Run;
using SAnalysisTreeListNode = AnalysisTreeListNode;
using SAnalysisTreeListNodeLine = AnalysisTreeListNodeLine;

public abstract record UIBuilder<T>
{
    public abstract T Build();
}

public static class UIBuilder
{
    public sealed record Run(
        string Text,
        ILazilyUpdatedBrush Brush,
        FontStyle FontStyle = FontStyle.Normal)
        : UIBuilder<ARun>
    {
        public override ARun Build()
        {
            return new(Text)
            {
                Foreground = Brush.Brush,
                FontStyle = FontStyle,
            };
        }
    }

    public sealed record AnalysisTreeListNodeLine(
        GroupedRunInlineCollection Inlines)
        : UIBuilder<SAnalysisTreeListNodeLine>
    {
        public AnalysisNodeKind AnalysisNodeKind { get; set; }

        public NodeTypeDisplay NodeTypeDisplay { get; set; }

        public TextSpanSource DisplaySpanSource { get; set; }
            = TextSpanSource.FullSpan;

        public bool IsLoading { get; set; }

        public AnalysisTreeListNodeLine(
            GroupedRunInlineCollection inlines,
            NodeTypeDisplay nodeTypeDisplay)
            : this(inlines)
        {
            NodeTypeDisplay = nodeTypeDisplay;
        }

        public override SAnalysisTreeListNodeLine Build()
        {
            return new()
            {
                GroupedRunInlines = Inlines.Build(),
                NodeTypeDisplay = NodeTypeDisplay,
                AnalysisNodeKind = AnalysisNodeKind,
                DisplaySpanSource = DisplaySpanSource,
                IsLoading = IsLoading,
            };
        }
    }

    public sealed record AnalysisTreeListNode(
        AnalysisTreeListNodeLine NodeLine,
        AnalysisNodeChildRetriever? ChildRetriever,
        object? AssociatedSyntaxObjectContent)
        : UIBuilder<SAnalysisTreeListNode>
    {
        public SyntaxObjectInfo? AssociatedSyntaxObject { get; }
            = SyntaxObjectInfo.GetInfoForObject(AssociatedSyntaxObjectContent);

        public Task<AnalysisTreeListNode>? NodeLoader { get; set; }

        public AnalysisTreeListNode WithAssociatedSyntaxObjectContent(object? content)
        {
            return new(NodeLine, ChildRetriever, content);
        }

        public override SAnalysisTreeListNode Build()
        {
            var node = new SAnalysisTreeListNode()
            {
                NodeLine = NodeLine.Build(),
                ChildRetriever = ChildRetriever,
                AssociatedSyntaxObject = AssociatedSyntaxObject,
            };

            if (NodeLoader is not null)
            {
                _ = node.LoadFromTask(NodeLoader);
            }

            return node;
        }
    }
}
