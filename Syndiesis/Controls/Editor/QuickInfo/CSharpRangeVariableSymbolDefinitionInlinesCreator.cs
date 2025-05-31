using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpRangeVariableSymbolDefinitionInlinesCreator(
    CSharpSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseCSharpSymbolDefinitionInlinesCreator<IRangeVariableSymbol>(parentContainer)
{
    public override GroupedRunInline.IBuilder CreateSymbolInline(IRangeVariableSymbol range)
    {
        throw new NotImplementedException(
            "This should not have been invoked given the hover context implementation");
    }

    protected override void CreateWithHoverContext(
        SymbolHoverContext context, ComplexGroupedRunInline.Builder inlines)
    {
        var explanation = Run("(range) ", CommonStyles.NullValueBrush);
        inlines.Add(explanation);

        var symbol = (IRangeVariableSymbol)context.Symbol!;
        var type = DiscoverRangeVariableType(symbol, context.SemanticModel);
        if (type is not null)
        {
            var typeCreator = ParentContainer.RootContainer.Commons
                .CreatorForSymbol(type);
            var typeInline = typeCreator.CreateSymbolInline(type);
            inlines.AddChild(typeInline);
            inlines.AddChild(CreateSpaceSeparatorRun());
        }

        var inline = ParentContainer.RootContainer.Commons
            .CreatorForSymbol(symbol).CreateSymbolInline(symbol);
        inlines.Add(inline.AsRunOrGrouped);
    }

    // Necessary due to limitations mentioned in
    // https://github.com/dotnet/roslyn/issues/78631
    private static ITypeSymbol? DiscoverRangeVariableType(
        IRangeVariableSymbol symbol, SemanticModel model)
    {
        var references = symbol.DeclaringSyntaxReferences.FirstOrDefault();
        if (references is null)
            return null;

        var syntax = references.GetSyntax();
        var expression = syntax.FirstAncestorOrSelf<QueryExpressionSyntax>();
        if (expression is null)
            return null;

        var nodes = expression.DescendantNodes()
            .Where(node
                => node is IdentifierNameSyntax identifier
                && identifier.Identifier.ValueText == symbol.Name)
            ;
        foreach (var node in nodes)
        {
            var symbolInfo = model.GetSymbolInfo(node);
            if (SymbolEqualityComparer.Default.Equals(symbolInfo.Symbol, symbol))
            {
                var typeInfo = model.GetTypeInfo(node);
                if (typeInfo.Type is not null)
                {
                    return typeInfo.Type;
                }
            }
        }
        return null;
    }
}
