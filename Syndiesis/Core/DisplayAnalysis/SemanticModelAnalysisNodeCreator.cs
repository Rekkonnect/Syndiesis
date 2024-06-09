using Avalonia.Media;
using Microsoft.CodeAnalysis;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Controls.Inlines;
using System.Collections.Generic;
using System.Linq;

namespace Syndiesis.Core.DisplayAnalysis;

using Run = UIBuilder.Run;
using AnalysisTreeListNode = UIBuilder.AnalysisTreeListNode;
using AnalysisTreeListNodeLine = UIBuilder.AnalysisTreeListNodeLine;

using GroupedRunInline = GroupedRunInline.IBuilder;
using SingleRunInline = SingleRunInline.Builder;
using SimpleGroupedRunInline = SimpleGroupedRunInline.Builder;
using ComplexGroupedRunInline = ComplexGroupedRunInline.Builder;

public sealed partial class SemanticModelAnalysisNodeCreator : BaseAnalysisNodeCreator
{
    private static readonly InterestingPropertyFilterCache _propertyCache
        = new(SemanticModelPropertyFilter.Instance);

    // node creators
    private readonly SemanticModelRootViewNodeCreator _semanticModelCreator;

    public SemanticModelAnalysisNodeCreator(
        BaseAnalysisNodeCreatorContainer parentContainer)
        : base(parentContainer)
    {
        _semanticModelCreator = new(this);
    }

    public override AnalysisTreeListNode? CreateRootViewNode(
        object? value,
        DisplayValueSource valueSource = default)
    {
        switch (value)
        {
            case SemanticModel semanticModel:
                return CreateRootSemanticModel(semanticModel, valueSource);

            default:
                break;
        }

        // fallback
        return ParentContainer.SyntaxCreator.CreateRootViewNode(value, valueSource);
    }

    private AnalysisTreeListNode CreateRootSemanticModel(
        SemanticModel semanticModel,
        DisplayValueSource valueSource)
    {
        return _semanticModelCreator.CreateNode(semanticModel, valueSource);
    }
}

partial class SemanticModelAnalysisNodeCreator
{
    public abstract class SemanticModelRootViewNodeCreator<TValue>(SemanticModelAnalysisNodeCreator creator)
        : RootViewNodeCreator<TValue, SemanticModelAnalysisNodeCreator>(creator)
    {
        public override AnalysisNodeKind GetNodeKind(TValue value)
        {
            return AnalysisNodeKind.Operation;
        }
    }

    public sealed class SemanticModelRootViewNodeCreator(SemanticModelAnalysisNodeCreator creator)
        : SemanticModelRootViewNodeCreator<SemanticModel>(creator)
    {
        public override AnalysisTreeListNodeLine CreateNodeLine(
            SemanticModel model, DisplayValueSource valueSource)
        {
            var type = model.GetType();
            var inline = NestedTypeDisplayGroupedRun(type);

            return AnalysisTreeListNodeLine(
                [inline],
                Styles.SemanticModelDisplay);
        }

        public override AnalysisNodeChildRetriever? GetChildRetriever(SemanticModel model)
        {
            return () => GetChildren(model);
        }

        private IReadOnlyList<AnalysisTreeListNode> GetChildren(SemanticModel model)
        {
            var type = _propertyCache.FilterForType(model.GetType());
            var properties = type.Properties;
            var preferredType = type.PreferredType;

            return properties
                .OrderBy(s => s.Name)
                .Select(property => CreateFromProperty(property, model))
                .ToList()
                ;
        }
    }
}

partial class SemanticModelAnalysisNodeCreator
{
    public static SemanticModelStyles Styles
        => AppSettings.Instance.StylePreferences.SemanticModelStyles!;

    public abstract class Types : CommonTypes
    {
        public const string SemanticModel = "SM";
    }

    public class SemanticModelStyles
    {
        public Color SemanticModelColor = CommonStyles.ClassMainColor;

        public NodeTypeDisplay SemanticModelDisplay
            => new(Types.SemanticModel, SemanticModelColor);
    }
}
