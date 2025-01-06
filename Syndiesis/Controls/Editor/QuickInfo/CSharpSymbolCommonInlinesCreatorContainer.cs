using Microsoft.CodeAnalysis;
using System;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpSymbolCommonInlinesCreatorContainer
    : BaseSymbolCommonInlinesCreatorContainer
{
    private readonly CSharpTypeCommonInlinesCreator _type;

    private readonly CSharpNamespaceCommonInlinesCreator _namespace;
    private readonly CSharpMethodCommonInlinesCreator _method;
    private readonly CSharpFieldCommonInlinesCreator _field;
    private readonly CSharpPropertyCommonInlinesCreator _property;
    private readonly CSharpEventCommonInlinesCreator _event;

    private readonly CSharpRangeVariableCommonInlinesCreator _rangeVariable;
    private readonly CSharpLocalCommonInlinesCreator _local;
    private readonly CSharpParameterCommonInlinesCreator _parameter;
    private readonly CSharpLabelCommonInlinesCreator _label;
    private readonly CSharpPreprocessingCommonInlinesCreator _preprocessing;

    public CSharpSymbolCommonInlinesCreatorContainer(
        ISymbolInlinesRootCreatorContainer rootContainer)
        : base(rootContainer)
    {
        _type = new(this);

        _namespace = new(this);
        _method = new(this);
        _field = new(this);
        _property = new(this);
        _event = new(this);

        _rangeVariable = new(this);
        _local = new(this);
        _parameter = new(this);
        _label = new(this);
        _preprocessing = new(this);
    }

    public override ISymbolItemInlinesCreator CreatorForSymbol<TSymbol>(TSymbol symbol)
    {
        switch (symbol)
        {
            case INamespaceSymbol:
                return _namespace;

            case ITypeSymbol:
                return _type;

            case IMethodSymbol:
                return _method;

            case IFieldSymbol:
                return _field;

            case IPropertySymbol:
                return _property;

            case IEventSymbol:
                return _event;

            case IPreprocessingSymbol:
                return _preprocessing;

            case IRangeVariableSymbol:
                return _rangeVariable;

            case ILocalSymbol:
                return _local;

            case IParameterSymbol:
                return _parameter;

            case ILabelSymbol:
                return _label;

            default:
                throw new ArgumentException("Invalid symbol kind");
        }
    }
}
