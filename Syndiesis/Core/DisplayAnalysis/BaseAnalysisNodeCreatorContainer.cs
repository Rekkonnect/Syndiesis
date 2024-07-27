using Microsoft.CodeAnalysis;
using System;

namespace Syndiesis.Core.DisplayAnalysis;

public abstract class BaseAnalysisNodeCreatorContainer
{
    public BaseSyntaxAnalysisNodeCreator SyntaxCreator { get; protected init; }
    public readonly AttributesAnalysisNodeCreator AttributeCreator;
    public readonly SymbolAnalysisNodeCreator SymbolCreator;
    public readonly OperationsAnalysisNodeCreator OperationCreator;
    public readonly SemanticModelAnalysisNodeCreator SemanticCreator;

    public BaseAnalysisNodeCreatorContainer()
    {
        AttributeCreator = new(this);
        SymbolCreator = new(this);
        OperationCreator = new(this);
        SemanticCreator = new(this);
    }

    public static BaseAnalysisNodeCreatorContainer CreateForLanguage(string languageName)
    {
        return languageName switch
        {
            LanguageNames.CSharp => new CSharpAnalysisNodeCreatorContainer(),
            LanguageNames.VisualBasic => new VisualBasicAnalysisNodeCreatorContainer(),
            _ => throw new NotSupportedException("Unsupported language"),
        };
    }
}
