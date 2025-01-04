using System;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class VisualBasicSymbolInlinesRootCreatorContainer
    : BaseSymbolInlinesRootCreatorContainer<
        VisualBasicSymbolDefinitionInlinesCreatorContainer,
        VisualBasicSymbolDefinitionInlinesCreatorContainer,
        VisualBasicSymbolDefinitionInlinesCreatorContainer,
        VisualBasicSymbolDefinitionInlinesCreatorContainer>
{
    protected override VisualBasicSymbolDefinitionInlinesCreatorContainer CreateCommonContainer(
        ISymbolInlinesRootCreatorContainer parentContainer)
    {
        return new(parentContainer);
    }

    protected override VisualBasicSymbolDefinitionInlinesCreatorContainer CreateDefinitionContainer(
        ISymbolInlinesRootCreatorContainer parentContainer)
    {
        throw new NotImplementedException();
    }

    protected override VisualBasicSymbolDefinitionInlinesCreatorContainer CreateDocsContainer(
        ISymbolInlinesRootCreatorContainer parentContainer)
    {
        throw new NotImplementedException();
    }

    protected override VisualBasicSymbolDefinitionInlinesCreatorContainer CreateExtraContainer(
        ISymbolInlinesRootCreatorContainer parentContainer)
    {
        throw new NotImplementedException();
    }
}
