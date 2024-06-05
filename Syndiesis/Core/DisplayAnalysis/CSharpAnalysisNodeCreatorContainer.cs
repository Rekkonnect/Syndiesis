namespace Syndiesis.Core.DisplayAnalysis;

public sealed class CSharpAnalysisNodeCreatorContainer : BaseAnalysisNodeCreatorContainer
{
    public CSharpAnalysisNodeCreatorContainer()
    {
        SyntaxCreator = new CSharpSyntaxAnalysisNodeCreator(this);
    }
}
