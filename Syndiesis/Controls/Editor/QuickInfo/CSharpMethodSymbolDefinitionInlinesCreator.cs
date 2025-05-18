using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpMethodSymbolDefinitionInlinesCreator(
    CSharpSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseCSharpSymbolDefinitionInlinesCreator<IMethodSymbol>(parentContainer)
{
    protected override ModifierInfo GetModifierInfo(IMethodSymbol symbol)
    {
        var info = base.GetModifierInfo(symbol);

        const MemberModifiers removedModifiers
            = MemberModifiers.Const
            | MemberModifiers.Volatile
            | MemberModifiers.FixedSizeBuffer
            | MemberModifiers.Scoped
            | MemberModifiers.Ref
            | MemberModifiers.RefReadOnly
            | MemberModifiers.In
            | MemberModifiers.Out
            ;

        return info with
        {
            Modifiers = info.Modifiers & ~removedModifiers
        };
    }

    public override void Create(IMethodSymbol method, ComplexGroupedRunInline.Builder inlines)
    {
        AddModifierInlines(method, inlines);
        ParentContainer.RootContainer.Commons
            .CreatorForSymbol(method).Create(method, inlines);
    }

    public override GroupedRunInline.IBuilder CreateSymbolInline(IMethodSymbol method)
    {
        return ParentContainer.RootContainer.Commons
            .CreatorForSymbol(method).CreateSymbolInline(method);
    }
}