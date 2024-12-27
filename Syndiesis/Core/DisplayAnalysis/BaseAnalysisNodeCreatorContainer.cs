using Microsoft.CodeAnalysis;
using System;

namespace Syndiesis.Core.DisplayAnalysis;

public abstract class BaseAnalysisNodeCreatorContainer
{
    public readonly BaseSyntaxAnalysisNodeCreator SyntaxCreator;
    public readonly AttributesAnalysisNodeCreator AttributeCreator;
    public readonly SymbolAnalysisNodeCreator SymbolCreator;
    public readonly OperationsAnalysisNodeCreator OperationCreator;
    public readonly SemanticModelAnalysisNodeCreator SemanticCreator;

    public BaseAnalysisNodeCreatorContainer()
    {
        SyntaxCreator = CreateSyntaxCreator();
        AttributeCreator = new(this);
        SymbolCreator = new(this);
        OperationCreator = new(this);
        SemanticCreator = new(this);
    }

    protected abstract BaseSyntaxAnalysisNodeCreator CreateSyntaxCreator(); 
    
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
