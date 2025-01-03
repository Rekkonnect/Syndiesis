using System;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpSymbolInlinesRootCreatorContainer
    : BaseSymbolInlinesRootCreatorContainer<
        CSharpSymbolDefinitionInlinesCreatorContainer,
        CSharpSymbolDefinitionInlinesCreatorContainer,
        CSharpSymbolDefinitionInlinesCreatorContainer,
        CSharpSymbolDefinitionInlinesCreatorContainer>
{
    protected override CSharpSymbolDefinitionInlinesCreatorContainer CreateCommonsContainer(
        ISymbolInlinesRootCreatorContainer parentContainer)
    {
        return new(parentContainer);
    }

    protected override CSharpSymbolDefinitionInlinesCreatorContainer CreateDefinitionContainer(
        ISymbolInlinesRootCreatorContainer parentContainer)
    {
        throw new NotImplementedException();
    }

    protected override CSharpSymbolDefinitionInlinesCreatorContainer CreateDocsContainer(
        ISymbolInlinesRootCreatorContainer parentContainer)
    {
        throw new NotImplementedException();
    }

    protected override CSharpSymbolDefinitionInlinesCreatorContainer CreateExtrasContainer(
        ISymbolInlinesRootCreatorContainer parentContainer)
    {
        throw new NotImplementedException();
    }
}
