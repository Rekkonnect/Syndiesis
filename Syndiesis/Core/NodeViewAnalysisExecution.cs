using Microsoft.CodeAnalysis;
using Syndiesis.Controls.AnalysisVisualization;
using Syndiesis.Core.DisplayAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace Syndiesis.Core;

public record NodeViewAnalysisRoot(
    SyntaxTree SyntaxTree,
    SyntaxNode? Node,
    SyntaxToken Token,
    SyntaxTrivia Trivia)
{
}

public sealed class NodeViewAnalysisExecution(
    Compilation? compilation,
    NodeViewAnalysisRoot? root)
{
    #region Value Sources
    private const string CurrentNodeName = "Node";
    private const string CurrentTokenName = "Token";
    private const string CurrentTriviaName = "Trivia";

    // value sources
    private static readonly DisplayValueSource _currentNodeValueSource
        = DisplayValueSource.Property(CurrentNodeName);

    private static readonly DisplayValueSource _currentTokenValueSource
        = DisplayValueSource.Property(CurrentTokenName);

    private static readonly DisplayValueSource _currentTriviaValueSource
        = DisplayValueSource.Property(CurrentTriviaName);

    private static readonly ComplexDisplayValueSource _parentValueSource
        = ConstructNodeAccessValueSource(
            DisplayValueSource.Property(nameof(SyntaxNode.Parent)));

    private static readonly ComplexDisplayValueSource _parentTriviaValueSource
        = ConstructNodeAccessValueSource(
            DisplayValueSource.Property(nameof(SyntaxNode.ParentTrivia)));

    private static readonly ComplexDisplayValueSource _childNodesValueSource
        = ConstructNodeAccessValueSource(
            DisplayValueSource.Method(nameof(SyntaxNode.ChildNodes)));

    private static readonly ComplexDisplayValueSource _childTokensValueSource
        = ConstructNodeAccessValueSource(
            DisplayValueSource.Method(nameof(SyntaxNode.ChildTokens)));

    private static readonly ComplexDisplayValueSource _childNodesAndTokensValueSource
        = ConstructNodeAccessValueSource(
            DisplayValueSource.Method(nameof(SyntaxNode.ChildNodesAndTokens)));

    private static readonly ComplexDisplayValueSource _getSymbolInfoValueSource
        = ConstructSemanticModelValueSource(nameof(ModelExtensions.GetSymbolInfo));

    private static readonly ComplexDisplayValueSource _getDeclaredSymbolInfoValueSource
        = ConstructSemanticModelValueSource(nameof(ModelExtensions.GetDeclaredSymbol));

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
    #endregion

    public readonly Compilation? Compilation = compilation;
    public readonly NodeViewAnalysisRoot? Root = root;

    private readonly SyntaxNode? _node = root?.Node;
    private readonly SemanticModel? _semanticModel
        = SemanticModelForTree(compilation, root?.SyntaxTree);

    private readonly BaseAnalysisNodeCreatorContainer _container =
        BaseAnalysisNodeCreatorContainer
            .CreateForLanguage(compilation?.Language ?? LanguageNames.CSharp);

    public static readonly NodeDetailsViewData InitializingData;

    static NodeViewAnalysisExecution()
    {
        var execution = new NodeViewAnalysisExecution(null, null);
        InitializingData = execution.ExecuteCore(default)!;
    }

    private static SemanticModel? SemanticModelForTree(Compilation? compilation, SyntaxTree? tree)
    {
        if (tree is null)
            return null;

        return compilation?.GetSemanticModel(tree);
    }

    private static ComplexDisplayValueSource ConstructNodeAccessValueSource(
        DisplayValueSource valueSource)
    {
        var methodSource = new ComplexDisplayValueSource(
            valueSource,
            null);

        return new(
            _currentNodeValueSource,
            methodSource);
    }

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
        var currentNode = syntaxCreator.CreateRootGeneral(_node, _currentNodeValueSource);
        var currentToken = syntaxCreator.CreateRootToken(
            Root?.Token ?? default, _currentTokenValueSource);
        var currentTrivia = syntaxCreator.CreateRootTrivia(
            Root?.Trivia ?? default, _currentTriviaValueSource);

        var parentNode = syntaxCreator.CreateRootGeneral(
            _node?.Parent, _parentValueSource, false);

        var parentTrivia = syntaxCreator.CreateRootTrivia(
            _node?.ParentTrivia ?? default, _parentTriviaValueSource, false);

        if (cancellationToken.IsCancellationRequested)
            return null;

        var childNodes = syntaxCreator.CreateLoadingNode(
            CreateChildNodesRootNode(cancellationToken),
            _childNodesValueSource);
        var childTokens = syntaxCreator.CreateLoadingNode(
            CreateChildTokensRootNode(cancellationToken),
            _childTokensValueSource);
        var childNodesAndTokens = syntaxCreator.CreateLoadingNode(
            CreateChildNodesAndTokensRootNode(cancellationToken),
            _childNodesAndTokensValueSource);

        if (cancellationToken.IsCancellationRequested)
            return null;

        var symbolInfo = syntaxCreator.CreateLoadingNode(
            CreateSymbolInfoRootNode(cancellationToken),
            _getSymbolInfoValueSource);
        var declaredSymbolInfo = syntaxCreator.CreateLoadingNode(
            CreateDeclaredSymbolInfoRootNode(cancellationToken),
            _getDeclaredSymbolInfoValueSource);
        var typeInfo = syntaxCreator.CreateLoadingNode(
            CreateTypeInfoRootNode(cancellationToken),
            _getTypeInfoValueSource);
        var aliasInfo = syntaxCreator.CreateLoadingNode(
            CreateAliasInfoRootNode(cancellationToken),
            _getAliasInfoValueSource);
        var preprocessingSymbolInfo = syntaxCreator.CreateLoadingNode(
            CreatePreprocessingSymbolInfoRootNode(cancellationToken),
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
            new(
                currentNode,
                currentToken,
                currentTrivia),

            new(
                parentNode,
                parentTrivia),

            new(
                childNodes,
                childTokens,
                childNodesAndTokens),

            new(
                symbolInfo,
                declaredSymbolInfo,
                typeInfo,
                aliasInfo,
                preprocessingSymbolInfo,
                conversion,
                operation)
            );
    }

    private async Task<UIBuilder.AnalysisTreeListNode?> CreateChildNodesRootNode(
        CancellationToken cancellationToken)
    {
        await BreatheAsync();
        if (cancellationToken.IsCancellationRequested)
            return null;
        return
            _container.SyntaxCreator.CreateRootNodeList(
                _node?.ChildNodes().ToList() ?? [],
                _childNodesValueSource);
    }

    private async Task<UIBuilder.AnalysisTreeListNode?> CreateChildTokensRootNode(
        CancellationToken cancellationToken)
    {
        await BreatheAsync();
        if (cancellationToken.IsCancellationRequested)
            return null;
        return
            _container.SyntaxCreator.CreateRootTokenList(
                _node?.ChildTokens().ToList() ?? [],
                _childTokensValueSource);
    }

    private async Task<UIBuilder.AnalysisTreeListNode?> CreateChildNodesAndTokensRootNode(
        CancellationToken cancellationToken)
    {
        await BreatheAsync();
        if (cancellationToken.IsCancellationRequested)
            return null;
        return
            _container.SyntaxCreator.CreateRootChildSyntaxList(
                _node?.ChildNodesAndTokens() ?? [],
                _childNodesAndTokensValueSource);
    }

    private async Task<UIBuilder.AnalysisTreeListNode?> CreateSymbolInfoRootNode(
        CancellationToken cancellationToken)
    {
        await BreatheAsync();
        if (cancellationToken.IsCancellationRequested)
            return null;

        var symbolInfo = ExecuteQuery(ModelExtensions.GetSymbolInfo, cancellationToken);
        return
            _container.SemanticCreator.CreateRootSymbolInfo(
                symbolInfo,
                _getSymbolInfoValueSource);
    }

    private async Task<UIBuilder.AnalysisTreeListNode?> CreateDeclaredSymbolInfoRootNode(
        CancellationToken cancellationToken)
    {
        await BreatheAsync();
        if (cancellationToken.IsCancellationRequested)
            return null;

        var declaredSymbol = ExecuteQuery(ModelExtensions.GetDeclaredSymbol, cancellationToken);
        return
            _container.SymbolCreator.CreateRootGeneral(
                declaredSymbol,
                _getDeclaredSymbolInfoValueSource)!;
    }

    private async Task<UIBuilder.AnalysisTreeListNode?> CreateTypeInfoRootNode(
        CancellationToken cancellationToken)
    {
        await BreatheAsync();
        if (cancellationToken.IsCancellationRequested)
            return null;

        var typeInfo = ExecuteQuery(ModelExtensions.GetTypeInfo, cancellationToken);
        return
            _container.SemanticCreator.CreateRootTypeInfo(
                typeInfo,
                _getTypeInfoValueSource);
    }

    private async Task<UIBuilder.AnalysisTreeListNode?> CreateAliasInfoRootNode(
        CancellationToken cancellationToken)
    {
        await BreatheAsync();

        var aliasSymbol = ExecuteQuery(ModelExtensions.GetAliasInfo, cancellationToken);
        return
            _container.SymbolCreator.CreateRootGeneral(
                aliasSymbol,
                _getAliasInfoValueSource)!;
    }

    private async Task<UIBuilder.AnalysisTreeListNode?> CreatePreprocessingSymbolInfoRootNode(
        CancellationToken cancellationToken)
    {
        await BreatheAsync();
        if (cancellationToken.IsCancellationRequested)
            return null;

        var preprocessingInfo = ExecuteQuery(GetPreprocessingSymbolInfo, cancellationToken);
        return
            _container.SemanticCreator.CreateRootPreprocessingSymbolInfo(
                preprocessingInfo,
                _getPreprocessingSymbolInfoValueSource);
    }

    private static PreprocessingSymbolInfo GetPreprocessingSymbolInfo(
        SemanticModel model, SyntaxNode node, CancellationToken cancellationToken)
    {
        return model.GetPreprocessingSymbolInfo(node);
    }

    private async Task<UIBuilder.AnalysisTreeListNode?> CreateConversionRootNode(
        CancellationToken cancellationToken)
    {
        await BreatheAsync();
        if (cancellationToken.IsCancellationRequested)
            return null;

        var conversion = ExecuteQuery(RoslynExtensions.GetConversionUnion, cancellationToken)
            ?? ConversionUnion.None
            ;
        return
            _container.SemanticCreator.CreateRootConversion(
                conversion,
                _getConversionValueSource);
    }

    private async Task<UIBuilder.AnalysisTreeListNode?> CreateOperationRootNode(
        CancellationToken cancellationToken)
    {
        await BreatheAsync();
        if (cancellationToken.IsCancellationRequested)
            return null;

        var operation = ExecuteQuery(GetOperation, cancellationToken);
        return
            _container.OperationCreator.CreateRootGeneral(
                operation,
                _getOperationValueSource)!;
    }

    private static IOperation? GetOperation(
        SemanticModel model, SyntaxNode node, CancellationToken cancellationToken)
    {
        return model.GetOperation(node, cancellationToken);
    }

    private static async Task BreatheAsync()
    {
        await Task.Delay(40);
    }

    private TResult? ExecuteQuery<TResult>(
        SemanticInfoQuery<TResult> query,
        CancellationToken cancellationToken)
    {
        if (_node is null)
            return default;

        if (_semanticModel is null)
            return default;

        return query(_semanticModel, _node, cancellationToken);
    }

    private delegate TResult? SemanticInfoQuery<TResult>(
        SemanticModel model,
        SyntaxNode node,
        CancellationToken cancellationToken);
}
