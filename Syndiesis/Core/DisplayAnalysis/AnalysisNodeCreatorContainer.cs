namespace Syndiesis.Core.DisplayAnalysis;

public sealed class AnalysisNodeCreatorContainer
{
    public AnalysisNodeCreationOptions Options { get; }

    public readonly SyntaxAnalysisNodeCreator SyntaxCreator;
    public readonly SymbolAnalysisNodeCreator SymbolCreator;
    public readonly OperationsAnalysisNodeCreator OperationCreator;
    public readonly SemanticModelAnalysisNodeCreator SemanticCreator;

    public AnalysisNodeCreatorContainer(AnalysisNodeCreationOptions options)
    {
        Options = options;

        SyntaxCreator = new(options, this);
        SymbolCreator = new(options, this);
        OperationCreator = new(options, this);
        SemanticCreator = new(options, this);
    }
}
