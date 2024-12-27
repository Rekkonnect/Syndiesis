namespace Syndiesis.Core.DisplayAnalysis;

public sealed class CSharpAnalysisNodeCreatorContainer : BaseAnalysisNodeCreatorContainer
{
    protected override BaseSyntaxAnalysisNodeCreator CreateSyntaxCreator()
    {
        return new CSharpSyntaxAnalysisNodeCreator(this);
    }
}
