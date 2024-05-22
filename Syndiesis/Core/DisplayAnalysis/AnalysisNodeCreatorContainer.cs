namespace Syndiesis.Core.DisplayAnalysis;

public sealed class AnalysisNodeCreatorContainer
{
    public AnalysisNodeCreationOptions Options { get; }

    public readonly SymbolAnalysisNodeCreator SymbolCreator;
    public readonly SyntaxAnalysisNodeCreator SyntaxCreator;
    public readonly OperationsAnalysisNodeCreator OperationCreator;
    public readonly SemanticModelAnalysisNodeCreator SemanticCreator;

    public AnalysisNodeCreatorContainer(AnalysisNodeCreationOptions options)
    {
        Options = options;

        SymbolCreator = new(options, this);
        SyntaxCreator = new(options, this);
        OperationCreator = new(options, this);
        SemanticCreator = new(options, this);
    }
}
