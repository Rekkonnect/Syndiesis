using Avalonia.Controls.Documents;
using Avalonia.Media;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Core.DisplayAnalysis;

using ARun = Run;
using SAnalysisTreeListNode = AnalysisTreeListNode;
using SAnalysisTreeListNodeLine = AnalysisTreeListNodeLine;

public abstract class UIBuilder<T>
{
    public abstract T Build();
}

public static class UIBuilder
{
    public sealed class Run(
        string text,
        ILazilyUpdatedBrush brush,
        FontStyle fontStyle = FontStyle.Normal)
        : UIBuilder<ARun>
    {
        public string Text { get; } = text;
        public ILazilyUpdatedBrush Brush { get; } = brush;
        public FontStyle FontStyle { get; } = fontStyle;

        public override ARun Build()
        {
            return new(Text)
            {
                Foreground = Brush.Brush,
                FontStyle = FontStyle,
            };
        }
    }

    public sealed class AnalysisTreeListNodeLine(
        GroupedRunInlineCollection inlines)
        : UIBuilder<SAnalysisTreeListNodeLine>
    {
        public GroupedRunInlineCollection Inlines { get; } = inlines;
        
        public AnalysisNodeKind AnalysisNodeKind { get; set; }

        public NodeTypeDisplay NodeTypeDisplay { get; set; }

        public TextSpanSource DisplaySpanSource { get; set; }
            = TextSpanSource.FullSpan;

        public AnalysisNodeLineContentState ContentState { get; set; }

        public AnalysisTreeListNodeLine(
            GroupedRunInlineCollection inlines,
            NodeTypeDisplay nodeTypeDisplay)
            : this(inlines)
        {
            NodeTypeDisplay = nodeTypeDisplay;
        }

        public override SAnalysisTreeListNodeLine Build()
        {
            var result = new SAnalysisTreeListNodeLine();
            BuildOnto(result);
            return result;
        }

        public void BuildOnto(SAnalysisTreeListNodeLine line)
        {
            line.GroupedRunInlines = Inlines.Build();
            line.NodeTypeDisplay = NodeTypeDisplay;
            line.AnalysisNodeKind = AnalysisNodeKind;
            line.DisplaySpanSource = DisplaySpanSource;
            line.ContentState = ContentState;
        }
    }

    public sealed class AnalysisTreeListNode(
        AnalysisTreeListNodeLine nodeLine,
        AnalysisNodeChildRetriever? childRetriever,
        object? associatedSyntaxObjectContent)
        : UIBuilder<SAnalysisTreeListNode>
    {
        public AnalysisTreeListNodeLine NodeLine { get; set; } = nodeLine;
        public AnalysisNodeChildRetriever? ChildRetriever { get; set; } = childRetriever;

        public SyntaxObjectInfo? AssociatedSyntaxObject { get; }
            = SyntaxObjectInfo.GetInfoForObject(associatedSyntaxObjectContent);

        public Task<AnalysisTreeListNode?>? NodeLoader { get; set; }
        public AnalysisTreeListNode? LoadingFailedNodeBuilder { get; set; }

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
                _ = LoadNodeFromTask(node);
            }

            return node;
        }

        private async Task LoadNodeFromTask(SAnalysisTreeListNode node)
        {
            await node.LoadFromTask(NodeLoader, LoadingFailedNodeBuilder);
        }
    }
}
