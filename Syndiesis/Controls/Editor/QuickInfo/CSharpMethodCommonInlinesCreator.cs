using Microsoft.CodeAnalysis;
using RoseLynn;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpMethodCommonInlinesCreator(
    CSharpSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseCSharpMemberCommonInlinesCreator<IMethodSymbol>(parentContainer)
{
    // TODO: Take care of this logic dissonance between the two creator methods
    public override void Create(IMethodSymbol method, ComplexGroupedRunInline.Builder inlines)
    {
        AddTargetModifier(method.RefKind is RefKind.RefReadOnly,  "ref readonly");
        AddTargetModifier(method.RefKind is RefKind.Ref,  "ref");

        bool showsReturnType = method.MethodKind
            is not MethodKind.Constructor
            and not MethodKind.StaticConstructor
            and not MethodKind.Conversion
            ; 
        if (showsReturnType)
        {
            var returnType = method.ReturnType;
            var typeCreator = ParentContainer.CreatorForSymbol(returnType);
            var returnTypeInline = typeCreator.CreateSymbolInline(returnType);
            inlines.AddChild(returnTypeInline);
            inlines.Add(CreateSpaceSeparatorRun());
        }
        
        base.Create(method, inlines);
        
        return;
        
        void AddTargetModifier(bool flag, string word)
        {
            AddModifier(inlines, flag, word);
        }
    }

    protected override GroupedRunInline.IBuilder CreateSymbolInlineCore(IMethodSymbol method)
    {
        var inlines = new ComplexGroupedRunInline.Builder();

        AddDisplayName(method, inlines);
        AddParameterList(method, inlines);

        return inlines;
    }

    private void AddDisplayName(IMethodSymbol method, ComplexGroupedRunInline.Builder inlines)
    {
        switch (method.MethodKind)
        {
            case MethodKind.Constructor:
            case MethodKind.StaticConstructor:
                AddConstructorName(method, inlines);
                break;
            
            case MethodKind.BuiltinOperator:
            case MethodKind.UserDefinedOperator:
                AddOperatorMethodInlines(method, inlines);
                break;
            
            case MethodKind.Conversion:
                AddConversionMethodInlines(method, inlines);
                break;
            
            default:
                AddOrdinaryMethodInlines(method, inlines);
                break;
        }
    }

    private void AddOrdinaryMethodInlines(IMethodSymbol method, ComplexGroupedRunInline.Builder inlines)
    {
        var nameInline = CreateNameAndGenericsInline(method);
        inlines.AddChild(nameInline);
    }

    private void AddConstructorName(IMethodSymbol method, ComplexGroupedRunInline.Builder inlines)
    {
        var type = method.ContainingType;
        var typeBrush = RoslynColorizationHelpers.BrushForTypeKind(ColorizationStyles, type.TypeKind)
            ?? CommonStyles.RawValueBrush;
        var typeName = type.Name;
        var typeRun = SingleRun(typeName, typeBrush);
        inlines.Add(typeRun);
    }
    
    private void AddParameterList(IMethodSymbol method, ComplexGroupedRunInline.Builder inlines)
    {
        var openParen = Run("(", CommonStyles.RawValueBrush);
        inlines.AddChild(openParen);
        var parameters = CreateParameterListInline(method.Parameters);
        inlines.AddNonNullChild(parameters);
        var closeParen = Run(")", CommonStyles.RawValueBrush);
        inlines.AddChild(closeParen);
    }

    private GroupedRunInline.IBuilder CreateNameAndGenericsInline(IMethodSymbol method)
    {
        var nameRun = SingleRun(method.Name, CommonStyles.MethodBrush);

        if (!method.IsGenericMethod)
        {
            return nameRun;
        }

        var inlines = new ComplexGroupedRunInline.Builder();

        inlines.Add(nameRun);
        AddTypeArgumentInlines(inlines, method.TypeArguments);
        return inlines;
    }

    private void AddConversionMethodInlines(IMethodSymbol method, ComplexGroupedRunInline.Builder inlines)
    {
        var operatorKind = OperatorKindFacts.MapNameToKind(method.Name, out var checkingMode);
        var modeKeyword = ConversionOperatorImplicationModeKeyword(operatorKind);
        var modeInline = SingleKeywordRun($"{modeKeyword} operator");
        inlines.Add(modeInline);
        inlines.Add(CreateSpaceSeparatorRun());

        var targetTypeInline = ParentContainer.AliasSimplifiedTypeCreator.CreateSymbolInline(method.ReturnType); 
        inlines.AddChild(targetTypeInline);
    }

    private static string ConversionOperatorImplicationModeKeyword(OperatorKind kind)
    {
        return kind switch
        {
            OperatorKind.Implicit => "implicit",
            OperatorKind.Explicit => "explicit",
            _ => "unknown",
        };
    }
    
    private static void AddOperatorMethodInlines(IMethodSymbol method, ComplexGroupedRunInline.Builder inlines)
    {
        var name = MethodNameOrOperatorSyntax(method.Name);
        inlines.AddChild(name);
    }
    
    private static GroupedRunInline.IBuilder MethodNameOrOperatorSyntax(string methodName)
    {
        var operatorKind = OperatorKindFacts.MapNameToKind(methodName, out var checkingMode);
        // If we don't know the operator symbol, just default to the method name
        // so we won't crash on future operators that are not yet supported
        var operatorSymbol = OperatorSymbol(operatorKind) ?? methodName;
        var nameRun = OperatorSymbolOrKeyword(operatorSymbol);
        var builder = new ComplexGroupedRunInline.Builder();
        
        var operatorKeyword = KeywordRun("operator");
        builder.Add(operatorKeyword);
        builder.Add(CreateSpaceSeparatorRun());

        if (checkingMode is OperatorCheckingMode.Checked)
        {
            var checkedKeyword = KeywordRun("checked");
            builder.Add(checkedKeyword);
            builder.Add(CreateSpaceSeparatorRun());
        }

        builder.Add(nameRun);
        
        return builder;
    }

    private static SingleRunInline.Builder OperatorSymbolOrKeyword(string symbol)
    {
        return symbol switch
        {
            "true" or "false" => SingleKeywordRun(symbol),
            _ => SingleRun(symbol, CommonStyles.MethodBrush),
        };
    }
    
    private static string? OperatorSymbol(OperatorKind kind)
    {
        return kind switch
        {
            OperatorKind.Addition => "+",
            OperatorKind.Subtraction => "-",
            OperatorKind.Multiply => "*",
            OperatorKind.Division => "/",
            OperatorKind.Modulus => "%",
            OperatorKind.BitwiseAnd => "&",
            OperatorKind.BitwiseOr => "|",
            OperatorKind.ExclusiveOr => "^",
            OperatorKind.LeftShift => "<<",
            OperatorKind.RightShift => ">>",
            OperatorKind.UnsignedRightShift => ">>>",
            OperatorKind.LogicalNot => "!",
            OperatorKind.UnaryNegation => "-",
            OperatorKind.Decrement => "--",
            OperatorKind.Increment => "++",
            OperatorKind.Equality => "==",
            OperatorKind.Inequality => "!=",
            OperatorKind.LessThan => "<",
            OperatorKind.GreaterThan => ">",
            OperatorKind.LessThanOrEqual => "<=",
            OperatorKind.GreaterThanOrEqual => ">=",
            OperatorKind.OnesComplement => "~",
            OperatorKind.UnaryPlus => "+",
            OperatorKind.True => "true",
            OperatorKind.False => "false",
            _ => null,
        };
    }
}
