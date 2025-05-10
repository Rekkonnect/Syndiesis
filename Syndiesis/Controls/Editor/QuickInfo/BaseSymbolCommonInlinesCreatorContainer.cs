using Microsoft.CodeAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

public abstract class BaseSymbolCommonInlinesCreatorContainer
    : BaseSymbolInlinesCreatorContainer
{
    private readonly CommonNamespaceCommonInlinesCreator _namespace;
    private readonly CommonFieldCommonInlinesCreator _field;
    private readonly CommonEventCommonInlinesCreator _event;

    private readonly CommonRangeVariableCommonInlinesCreator _rangeVariable;
    private readonly CommonLocalCommonInlinesCreator _local;
    private readonly CommonParameterCommonInlinesCreator _parameter;
    private readonly CommonLabelCommonInlinesCreator _label;
    private readonly CommonPreprocessingCommonInlinesCreator _preprocessing;
    private readonly CommonAliasCommonInlinesCreator _alias;

    protected BaseSymbolCommonInlinesCreatorContainer(
        ISymbolInlinesRootCreatorContainer rootContainer)
        : base(rootContainer)
    {
        _namespace = new(this);
        _field = new(this);
        _event = new(this);

        _rangeVariable = new(this);
        _local = new(this);
        _parameter = new(this);
        _label = new(this);
        _preprocessing = new(this);
        _alias = new(this);
    }

    public sealed override ISymbolItemInlinesCreator CreatorForSymbol<TSymbol>(TSymbol symbol)
    {
        switch (symbol)
        {
            case INamespaceSymbol:
                return _namespace;

            case IFieldSymbol:
                return _field;

            case IEventSymbol:
                return _event;

            case IPreprocessingSymbol:
                return _preprocessing;

            case IAliasSymbol:
                return _alias;

            case IRangeVariableSymbol:
                return _rangeVariable;

            case ILocalSymbol:
                return _local;

            case IParameterSymbol:
                return _parameter;

            case ILabelSymbol:
                return _label;

            default:
                return FallbackCreatorForSymbol(symbol);
        }
    }

    protected abstract ISymbolItemInlinesCreator FallbackCreatorForSymbol<TSymbol>(TSymbol symbol)
        where TSymbol : class, ISymbol;
}
