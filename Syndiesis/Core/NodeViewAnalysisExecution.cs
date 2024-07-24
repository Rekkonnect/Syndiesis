using Microsoft.CodeAnalysis;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Core.DisplayAnalysis;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Syndiesis.Core;

public sealed class NodeViewAnalysisExecution(Compilation compilation, SyntaxNode node)
{
    private const string CurrentNodeName = "Node";

    // value sources
    private static readonly DisplayValueSource _currentNodeValueSource
        = DisplayValueSource.Property(CurrentNodeName);

    private static readonly DisplayValueSource _parentValueSource
        = DisplayValueSource.Property(nameof(SyntaxNode.Parent));

    private static readonly DisplayValueSource _childNodesValueSource
        = DisplayValueSource.Method(nameof(SyntaxNode.ChildNodes));

    private static readonly DisplayValueSource _childTokensValueSource
        = DisplayValueSource.Method(nameof(SyntaxNode.ChildTokens));

    private static readonly DisplayValueSource _childNodesAndTokensValueSource
        = DisplayValueSource.Method(nameof(SyntaxNode.ChildNodesAndTokens));

    private static readonly ComplexDisplayValueSource _getSymbolInfoValueSource
        = ConstructSemanticModelValueSource(nameof(ModelExtensions.GetSymbolInfo));

    private static readonly ComplexDisplayValueSource _getTypeInfoValueSource
        = ConstructSemanticModelValueSource(nameof(ModelExtensions.GetTypeInfo));

    private static readonly ComplexDisplayValueSource _getAliasInfoValueSource
        = ConstructSemanticModelValueSource(nameof(ModelExtensions.GetAliasInfo));

    private static readonly ComplexDisplayValueSource _getPreprocessingSymbolInfoValueSource
        = ConstructSemanticModelValueSource(nameof(SemanticModel.GetPreprocessingSymbolInfo));

    // for both C# and VB there is a GetConversion method specific to that language
    private static readonly ComplexDisplayValueSource _getConversionValueSource
        = ConstructSemanticModelValueSource(nameof(
            Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetConversion));

    private static readonly ComplexDisplayValueSource _getOperationValueSource
        = ConstructSemanticModelValueSource(nameof(SemanticModel.GetOperation));

    private readonly Compilation _compilation = compilation;
    private readonly SyntaxNode _node = node;

    private readonly SemanticModel _semanticModel = compilation.GetSemanticModel(node.SyntaxTree);

    private readonly BaseAnalysisNodeCreatorContainer _container =
        BaseAnalysisNodeCreatorContainer
            .CreateForLanguage(compilation.Language);

    private static ComplexDisplayValueSource ConstructSemanticModelValueSource(
        string methodName)
    {
        var methodSource = new ComplexDisplayValueSource(
            DisplayValueSource.Method(methodName),
            null)
        {
            Arguments = ImmutableArray.Create<ComplexDisplayValueSource>(
                [DisplayValueSource.Property(CurrentNodeName)]),
        };

        return new(
            DisplayValueSource.This,
            methodSource);
    }

    public NodeDetailsViewData? ExecuteCore(CancellationToken cancellationToken)
    {
        var syntaxCreator = _container.SyntaxCreator;
        var currentNode = syntaxCreator.CreateRootNode(_node, _currentNodeValueSource);
        var parentNode = syntaxCreator.CreateRootGeneral(
            _node.Parent, _parentValueSource, false);

        if (cancellationToken.IsCancellationRequested)
            return null;

        var childNodes = syntaxCreator.CreateLoadingNode(
            CreateChildNodesRootNode(),
            _childNodesValueSource);
        var childTokens = syntaxCreator.CreateLoadingNode(
            CreateChildTokensRootNode(),
            _childTokensValueSource);
        var childNodesAndTokens = syntaxCreator.CreateLoadingNode(
            CreateChildNodesAndTokensRootNode(),
            _childNodesAndTokensValueSource);

        if (cancellationToken.IsCancellationRequested)
            return null;

        var symbolInfo = syntaxCreator.CreateLoadingNode(
            CreateSymbolInfoRootNode(cancellationToken),
            _getSymbolInfoValueSource);
        var typeInfo = syntaxCreator.CreateLoadingNode(
            CreateTypeInfoRootNode(cancellationToken),
            _getTypeInfoValueSource);
        var aliasInfo = syntaxCreator.CreateLoadingNode(
            CreateAliasInfoRootNode(cancellationToken),
            _getAliasInfoValueSource);
        var preprocessingSymbolInfo = syntaxCreator.CreateLoadingNode(
            CreatePreprocessingSymbolInfoRootNode(),
            _getPreprocessingSymbolInfoValueSource);
        var conversion = syntaxCreator.CreateLoadingNode(
            CreateConversionRootNode(cancellationToken),
            _getConversionValueSource);
        var operation = syntaxCreator.CreateLoadingNode(
            CreateOperationRootNode(cancellationToken),
            _getOperationValueSource);

        if (cancellationToken.IsCancellationRequested)
            return null;

        return new(
            new(currentNode),

            new(parentNode),

            new(
                childNodes,
                childTokens,
                childNodesAndTokens),

            new(
                symbolInfo,
                typeInfo,
                aliasInfo,
                preprocessingSymbolInfo,
                conversion,
                operation)
            );
    }

    public static readonly NodeDetailsViewData InitializingData = CreateInitializingData();

    private static NodeDetailsViewData CreateInitializingData()
    {
        var creatorContainer = BaseAnalysisNodeCreatorContainer
            .CreateForLanguage(LanguageNames.CSharp);

        var syntaxCreator = creatorContainer.SyntaxCreator;

        var currentNode = syntaxCreator
            .CreateLoadingNode(null, _currentNodeValueSource);
        var parentNode = syntaxCreator
            .CreateLoadingNode(null, _parentValueSource);
        var childNodes = syntaxCreator
            .CreateLoadingNode(null, _childNodesValueSource);
        var childTokens = syntaxCreator
            .CreateLoadingNode(null, _childTokensValueSource);
        var childNodesAndTokens = syntaxCreator
            .CreateLoadingNode(null, _childNodesAndTokensValueSource);

        var symbolInfo = syntaxCreator
            .CreateLoadingNode(null, _getSymbolInfoValueSource);
        var typeInfo = syntaxCreator
            .CreateLoadingNode(null, _getTypeInfoValueSource);
        var aliasInfo = syntaxCreator
            .CreateLoadingNode(null, _getAliasInfoValueSource);
        var preprocessingSymbolInfo = syntaxCreator
            .CreateLoadingNode(null, _getPreprocessingSymbolInfoValueSource);
        var conversion = syntaxCreator
            .CreateLoadingNode(null, _getConversionValueSource);
        var operation = syntaxCreator
            .CreateLoadingNode(null, _getOperationValueSource);

        return new(
            new(currentNode),

            new(parentNode),

            new(
                childNodes,
                childTokens,
                childNodesAndTokens),

            new(
                symbolInfo,
                typeInfo,
                aliasInfo,
                preprocessingSymbolInfo,
                conversion,
                operation)
            );
    }

    private async Task<UIBuilder.AnalysisTreeListNode> CreateChildNodesRootNode()
    {
        await BreatheAsync();
        return
            _container.SyntaxCreator.CreateRootNodeList(
                _node.ChildNodes().ToList(),
                _childNodesValueSource);
    }

    private async Task<UIBuilder.AnalysisTreeListNode> CreateChildTokensRootNode()
    {
        await BreatheAsync();
        return
            _container.SyntaxCreator.CreateRootTokenList(
                new SyntaxTokenList(_node.ChildTokens()),
                _childTokensValueSource);
    }

    private async Task<UIBuilder.AnalysisTreeListNode> CreateChildNodesAndTokensRootNode()
    {
        await BreatheAsync();
        return
            _container.SyntaxCreator.CreateRootChildSyntaxList(
                _node.ChildNodesAndTokens(),
                _childNodesAndTokensValueSource);
    }

    private async Task<UIBuilder.AnalysisTreeListNode> CreateSymbolInfoRootNode(
        CancellationToken cancellationToken)
    {
        await BreatheAsync();
        return
            _container.SemanticCreator.CreateRootSymbolInfo(
                _semanticModel.GetSymbolInfo(_node, cancellationToken),
                _getSymbolInfoValueSource);
    }

    private async Task<UIBuilder.AnalysisTreeListNode> CreateTypeInfoRootNode(
        CancellationToken cancellationToken)
    {
        await BreatheAsync();
        return
            _container.SemanticCreator.CreateRootTypeInfo(
                _semanticModel.GetTypeInfo(_node, cancellationToken),
                _getTypeInfoValueSource);
    }

    private async Task<UIBuilder.AnalysisTreeListNode> CreateAliasInfoRootNode(
        CancellationToken cancellationToken)
    {
        await BreatheAsync();
        return
            _container.SymbolCreator.CreateRootGeneral(
                _semanticModel.GetAliasInfo(_node, cancellationToken),
                _getAliasInfoValueSource)!;
    }

    private async Task<UIBuilder.AnalysisTreeListNode> CreatePreprocessingSymbolInfoRootNode()
    {
        await BreatheAsync();
        return
            _container.SemanticCreator.CreateRootPreprocessingSymbolInfo(
                _semanticModel.GetPreprocessingSymbolInfo(_node),
                _getPreprocessingSymbolInfoValueSource);
    }

    private async Task<UIBuilder.AnalysisTreeListNode> CreateConversionRootNode(
        CancellationToken cancellationToken)
    {
        await BreatheAsync();
        return
            _container.SemanticCreator.CreateRootConversion(
                _semanticModel.GetConversionUnion(_node, cancellationToken),
                _getConversionValueSource);
    }

    private async Task<UIBuilder.AnalysisTreeListNode> CreateOperationRootNode(
        CancellationToken cancellationToken)
    {
        await BreatheAsync();
        return
            _container.OperationCreator.CreateRootGeneral(
                _semanticModel.GetOperation(_node, cancellationToken),
                _getOperationValueSource)!;
    }

    private static async Task BreatheAsync()
    {
        await Task.Delay(40);
    }
}
