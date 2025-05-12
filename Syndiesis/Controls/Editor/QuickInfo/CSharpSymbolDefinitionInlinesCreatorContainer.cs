using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpSymbolDefinitionInlinesCreatorContainer
    : BaseSymbolDefinitionInlinesCreatorContainer
{
    public readonly CSharpNamedTypeSymbolDefinitionInlinesCreator NamedTypeCreator;
    public readonly CSharpMethodSymbolDefinitionInlinesCreator MethodCreator;
    
    public readonly CSharpFieldSymbolDefinitionInlinesCreator FieldCreator;
    public readonly CSharpPropertySymbolDefinitionInlinesCreator PropertyCreator;
    public readonly CSharpEventSymbolDefinitionInlinesCreator EventCreator;
    public readonly CSharpRangeVariableSymbolDefinitionInlinesCreator RangeCreator;
    public readonly CSharpLabelSymbolDefinitionInlinesCreator LabelCreator;
    public readonly CSharpParameterSymbolDefinitionInlinesCreator ParameterCreator;
    public readonly CSharpLocalSymbolDefinitionInlinesCreator LocalCreator;
    public readonly CSharpDiscardSymbolDefinitionInlinesCreator DiscardCreator;

    public CSharpSymbolDefinitionInlinesCreatorContainer(
        ISymbolInlinesRootCreatorContainer rootContainer)
        : base(rootContainer)
    {
        NamedTypeCreator = new(this);
        MethodCreator = new(this);
        FieldCreator = new(this);
        PropertyCreator = new(this);
        EventCreator = new(this);
        RangeCreator = new(this);
        LabelCreator = new(this);
        ParameterCreator = new(this);
        LocalCreator = new(this);
        DiscardCreator = new(this);
    }

    protected override ISymbolItemInlinesCreator FallbackCreatorForSymbol<TSymbol>(TSymbol symbol)
    {
        switch (symbol)
        {
            case INamedTypeSymbol:
                return NamedTypeCreator;

            case IMethodSymbol:
                return MethodCreator;

            case IFieldSymbol:
                return FieldCreator;

            case IPropertySymbol:
                return PropertyCreator;

            case IEventSymbol:
                return EventCreator;

            case IRangeVariableSymbol:
                return RangeCreator;

            case ILabelSymbol:
                return LabelCreator;

            case IParameterSymbol:
                return ParameterCreator;

            case ILocalSymbol:
                return LocalCreator;

            case IDiscardSymbol:
                return DiscardCreator;

            default:
                return RootContainer.Commons.CreatorForSymbol(symbol);
        }
    }
}
