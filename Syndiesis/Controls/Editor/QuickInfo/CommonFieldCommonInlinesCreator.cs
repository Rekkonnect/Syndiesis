using Microsoft.CodeAnalysis;
using Syndiesis.Core;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CommonFieldCommonInlinesCreator(
    BaseSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseSimpleTypedNamedCommonInlinesCreator<IFieldSymbol>(parentContainer)
{
    protected override ILazilyUpdatedBrush GetBrush(IFieldSymbol symbol)
    {
        if (symbol.IsConst)
            return CommonStyles.ConstantMainBrush;

        if (symbol.IsEnumField())
            return CommonStyles.EnumFieldMainBrush;

        return ColorizationStyles.FieldBrush;
    }
}
