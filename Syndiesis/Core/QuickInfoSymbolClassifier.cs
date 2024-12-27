using Microsoft.CodeAnalysis;

namespace Syndiesis.Core;

public static class QuickInfoSymbolClassifier
{
    public static QuickInfoSymbolClassification ClassifySymbol(ISymbol symbol)
    {
        if (symbol.IsConstant())
            return QuickInfoSymbolClassification.Constant;

        if (symbol.IsEnumField())
            return QuickInfoSymbolClassification.EnumField;

        switch (symbol)
        {
            case INamespaceSymbol:
                return QuickInfoSymbolClassification.Namespace;
            case IAliasSymbol:
                return QuickInfoSymbolClassification.Alias;
            case IAssemblySymbol:
                return QuickInfoSymbolClassification.Assembly;
            case IModuleSymbol:
                return QuickInfoSymbolClassification.Module;
            
            case INamedTypeSymbol named:
                switch (named.TypeKind)
                {
                    case TypeKind.Class:
                        return QuickInfoSymbolClassification.Class;
                    case TypeKind.Struct:
                        return QuickInfoSymbolClassification.Struct;
                    case TypeKind.Interface:
                        return QuickInfoSymbolClassification.Interface;
                    case TypeKind.Enum:
                        return QuickInfoSymbolClassification.Enum;
                    case TypeKind.Delegate:
                        return QuickInfoSymbolClassification.Delegate;
                    case TypeKind.Error:
                        return QuickInfoSymbolClassification.Error;
                }

                break;
            
            case IEventSymbol:
                return QuickInfoSymbolClassification.Event;
            case IFieldSymbol:
                return QuickInfoSymbolClassification.Field;
            case IPropertySymbol:
                return QuickInfoSymbolClassification.Property;
            
            case IMethodSymbol method:
                if (method.IsOperator())
                    return QuickInfoSymbolClassification.Operator;
                
                if (method.MethodKind is MethodKind.Conversion)
                    return QuickInfoSymbolClassification.Conversion;
                
                return QuickInfoSymbolClassification.Method;
            
            case ILabelSymbol:
                return QuickInfoSymbolClassification.Label;
            case ILocalSymbol:
                return QuickInfoSymbolClassification.Local;
            case IParameterSymbol:
                return QuickInfoSymbolClassification.Parameter;
            case ITypeParameterSymbol:
                return QuickInfoSymbolClassification.TypeParameter;
            case IPreprocessingSymbol:
                return QuickInfoSymbolClassification.Preprocessing;
        }
        
        return QuickInfoSymbolClassification.None;
    }
}
