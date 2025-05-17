using Microsoft.CodeAnalysis;
using Serilog;
using Syndiesis.ColorHelpers;
using Syndiesis.Controls.Inlines;
using Syndiesis.Core;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection.Metadata;

namespace Syndiesis.Controls.Editor.QuickInfo;

public class CSharpTypeCommonInlinesCreator(
    CSharpSymbolCommonInlinesCreatorContainer parentContainer)
    : BaseCSharpMemberCommonInlinesCreator<ITypeSymbol>(parentContainer)
{
    public override GroupedRunInline.IBuilder CreateSymbolInline(ITypeSymbol symbol)
    {
        return CreateTypeInline(symbol);
    }

    private GroupedRunInline.IBuilder CreateTypeInline(ITypeSymbol type)
    {
        switch (type)
        {
            case IArrayTypeSymbol array:
            {
                var nested = array.ElementType;
                var rankSpecifier = CreateArrayRankDisplay(array.Rank);
                return SuffixedTypeDisplay(nested, rankSpecifier);
            }

            case IPointerTypeSymbol pointer:
            {
                var nested = pointer.PointedAtType;
                return SuffixedTypeDisplay(nested, "*");
            }

            case IFunctionPointerTypeSymbol functionPointer:
            {
                return CreateFunctionPointerInline(functionPointer);
            }

            case ITypeParameterSymbol typeParameter:
            {
                return CreateTypeParameterInline(typeParameter);
            }

            case INamedTypeSymbol named:
            {
                return CreateNamedTypeInline(named);
            }
            
            case IDynamicTypeSymbol:
            {
                return CreateDynamicInline();
            }

            default:
            {
                return CreateFallbackTypeInline(type);
            }
        }
    }

    private GroupedRunInline.IBuilder CreateFallbackTypeInline(ITypeSymbol type)
    {
        if (type.IsTupleType)
        {
            return CreateTupleInline(type);
        }

        if (type.IsAnonymousType)
        {
            return CreateAnonymousInline(type);
        }

        if (type.IsExtension)
        {
            return CreateExtensionTypeInline(type);
        }

        Log.Error("Found unimplemented fallback symbol type kind {typeKind}", type.TypeKind);
        return new SimpleGroupedRunInline.Builder();
    }

    private static GroupedRunInline.IBuilder CreateTypeParameterInline(ITypeParameterSymbol type)
    {
        var inlines = new ComplexGroupedRunInline.Builder();

        switch (type.Variance)
        {
            case VarianceKind.In:
                AddVariance("in");
                break;

            case VarianceKind.Out:
                AddVariance("out");
                break;
        }

        var run = SingleRun(type.Name, ColorizationStyles.TypeParameterBrush);
        inlines.Add(run);
        return inlines;

        void AddVariance(string keyword)
        {
            var varianceRun = Run(keyword, CommonStyles.KeywordBrush);
            inlines.AddChild(varianceRun);
            inlines.AddChild(CreateSpaceSeparatorRun());
        }
    }

    private GroupedRunInline.IBuilder CreateFunctionPointerInline(IFunctionPointerTypeSymbol type)
    {
        var signature = type.Signature;

        var inlines = new ComplexGroupedRunInline.Builder();

        var delegateRun = KeywordRun("delegate");
        inlines.AddChild(delegateRun);

        var rawBrush = CommonStyles.RawValueBrush;

        var asteriskRun = Run("*", rawBrush);
        inlines.AddChild(asteriskRun);
        if (signature.CallingConvention is not SignatureCallingConvention.Default)
        {
            var unmanagedRun = KeywordRun(" unmanaged");
            inlines.AddChild(unmanagedRun);

            var callingConventionNames = GetConventionNameList(signature);
            if (callingConventionNames.Length > 0)
            {
                var callingConventionRuns = new List<RunOrGrouped>();
                var callingConventionListStart = Run("[", rawBrush);
                callingConventionRuns.Add(callingConventionListStart);

                for (var i = 0; i < callingConventionNames.Length; i++)
                {
                    if (i > 0)
                    {
                        var separator = CreateArgumentSeparatorRun();
                        callingConventionRuns.Add(separator);
                    }
                    
                    var conventionName = callingConventionNames[i];
                    // Hard-code the class brush since the calling convention types are all classes
                    var run = Run(conventionName, CommonStyles.ClassMainBrush);
                    var conventionInline = new SingleRunInline.Builder(run);
                    callingConventionRuns.Add(conventionInline);
                }

                var callingConventionListEnd = Run("]", rawBrush);
                callingConventionRuns.Add(callingConventionListEnd);

                var callingConventionInline = new ComplexGroupedRunInline.Builder(callingConventionRuns);
                inlines.Add(callingConventionInline);
            }
        }

        var openingTag = Run("<", rawBrush);
        inlines.AddChild(openingTag);
        var parameters = signature.Parameters;
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            var inner = CreateSymbolInline(parameter.Type);
            inlines.AddChild(inner);

            var separator = CreateArgumentSeparatorRun();
            inlines.AddChild(separator);
        }

        var returnTypeInline = CreateSymbolInline(signature.ReturnType);
        inlines.AddChild(returnTypeInline);

        var closingTag = Run(">", rawBrush);
        inlines.AddChild(closingTag);

        return inlines;
    }

    private static ImmutableArray<string> GetConventionNameList(IMethodSymbol signature)
    {
        if (signature.UnmanagedCallingConventionTypes is not [] and var types)
        {
            return types.Select(DisplayForUnmanagedCallingConventionType).ToImmutableArray();
        }
        
        return GetConventionNameList(signature.CallingConvention);
    }

    private static ImmutableArray<string> GetConventionNameList(SignatureCallingConvention convention)
    {
        var conventionName = GetConventionName(convention);
        if (conventionName is null)
        {
            return [];
        }
        
        return [conventionName];
    }

    private static string? GetConventionName(SignatureCallingConvention convention)
    {
        return convention switch
        {
            SignatureCallingConvention.ThisCall => KnownIdentifierHelpers.CallingConventions.ThisCall,
            SignatureCallingConvention.FastCall => KnownIdentifierHelpers.CallingConventions.FastCall,
            SignatureCallingConvention.StdCall => KnownIdentifierHelpers.CallingConventions.StdCall,
            SignatureCallingConvention.CDecl => KnownIdentifierHelpers.CallingConventions.CDecl,
            _ => null,
        };
    }

    private static string DisplayForUnmanagedCallingConventionType(INamedTypeSymbol type)
    {
        // Assuming that the name is CallConvXyz, for example CallConvCdecl
        const string prefix = "CallConv";
        int prefixLength = prefix.Length;
        return type.Name[prefixLength..];
    }

    private GroupedRunInline.IBuilder CreateNamedTypeInline(INamedTypeSymbol type)
    {
        if (type.IsExtension)
        {
            return CreateExtensionTypeInline(type);
        }

        return CreateOrdinaryNamedTypeInline(type);
    }

    // Due to the extensions feature being in preview, the core logic is defined
    // assuming that ITypeSymbol could theoretically represent an extension type
    // despite the chances of the API changing are minimal
    private ComplexGroupedRunInline.Builder CreateExtensionTypeInline(ITypeSymbol type)
    {
        var inlines = new ComplexGroupedRunInline.Builder();
        AddKeywordRun("extension", inlines);
        var container = (CSharpSymbolCommonInlinesCreatorContainer)ParentContainer.RootContainer.Commons;
        if (type is INamedTypeSymbol named)
        {
            container.AliasSimplifiedTypeCreator.AddTypeArgumentInlines(inlines, named.TypeArguments);
        }

        var rawBrush = CommonStyles.RawValueBrush;
        inlines.Add(Run("(", rawBrush));

        var parameter = type.ExtensionParameter;
        if (parameter is not null)
        {
            var parameterType = parameter.Type;
            var parameterInline = ParentContainer.CreatorForSymbol(parameterType)
                .CreateSymbolInline(parameterType);
            inlines.Add(parameterInline.AsRunOrGrouped);
        }

        inlines.Add(Run(")", rawBrush));

        return inlines;
    }

    private GroupedRunInline.IBuilder CreateOrdinaryNamedTypeInline(INamedTypeSymbol type)
    {
        var typeBrush = RoslynColorizationHelpers.BrushForTypeKind(ColorizationStyles, type.TypeKind)
            ?? CommonStyles.RawValueBrush;
        var symbolRun = SingleRun(type.Name, typeBrush);

        if (!type.IsGenericType)
        {
            return symbolRun;
        }

        var inlines = new ComplexGroupedRunInline.Builder();

        inlines.Add(symbolRun);
        AddTypeArgumentInlines(inlines, type.TypeArguments);
        return inlines;
    }

    private GroupedRunInline.IBuilder CreateTupleInline(ITypeSymbol tupleType)
    {
        var fields = tupleType.GetFields();

        var runs = new List<RunOrGrouped>();
        var openParenthesisRun = Run("(", CommonStyles.RawValueBrush);
        var closeParenthesisRun = Run(")", CommonStyles.RawValueBrush);

        runs.Add(openParenthesisRun);

        for (int i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            var fieldInline = CreateTupleTypeFieldInline(field);
            runs.Add(new(fieldInline));

            if (i < fields.Length - 1)
            {
                var separator = CreateArgumentSeparatorRun();
                runs.Add(separator);
            }
        }

        runs.Add(closeParenthesisRun);

        return new ComplexGroupedRunInline.Builder(runs);
    }

    private GroupedRunInline.IBuilder CreateTupleTypeFieldInline(IFieldSymbol field)
    {
        Contract.Assert(field.ContainingType.IsTupleType);
        var left = CreateSymbolInline(field.Type);

        var fieldBrush = ColorizationStyles.FieldBrush;
        ILazilyUpdatedBrush brush = fieldBrush;
        if (!field.IsExplicitlyNamedTupleElement)
        {
            var fadeTransformation = new HsvTransformation(Value: -0.4);
            brush = fieldBrush.WithHsvTransformation(fadeTransformation);
        }

        var nameRun = Run(field.Name, brush);
        var suffixGroup = new SimpleGroupedRunInline.Builder([nameRun]);
        return new ComplexGroupedRunInline.Builder([new(left), suffixGroup]);
    }

    private GroupedRunInline.IBuilder CreateAnonymousInline(ITypeSymbol anonymousType)
    {
        var properties = anonymousType.GetProperties();
        if (properties.Length is 0)
            return CreateEmptyAnonymousInline();

        var runs = new List<RunOrGrouped>();
        var newRun = Run("new", CommonStyles.KeywordBrush);
        var leftBraceRun = Run(" { ", CommonStyles.RawValueBrush);
        var rightBraceRun = Run(" }", CommonStyles.RawValueBrush);

        runs.Add(newRun);
        runs.Add(leftBraceRun);

        for (int i = 0; i < properties.Length; i++)
        {
            var property = properties[i];
            var propertyInline = CreateAnonymousTypePropertyInline(property);
            runs.Add(new(propertyInline));

            if (i < properties.Length - 1)
            {
                var separator = CreateArgumentSeparatorRun();
                runs.Add(separator);
            }
        }

        runs.Add(rightBraceRun);

        return new ComplexGroupedRunInline.Builder(runs);
    }

    private GroupedRunInline.IBuilder CreateEmptyAnonymousInline()
    {
        var newRun = Run("new", CommonStyles.KeywordBrush);
        var bracesRun = Run(" { }", CommonStyles.RawValueBrush);
        return new SimpleGroupedRunInline.Builder([newRun, bracesRun]);
    }

    private GroupedRunInline.IBuilder CreateAnonymousTypePropertyInline(IPropertySymbol property)
    {
        Contract.Assert(property.ContainingType.IsAnonymousType);
        var left = CreateSymbolInline(property.Type);
        var nameRun = Run(property.Name, CommonStyles.PropertyBrush);
        var suffixGroup = new SimpleGroupedRunInline.Builder([nameRun]);
        return new ComplexGroupedRunInline.Builder([new(left), suffixGroup]);
    }

    private GroupedRunInline.IBuilder SuffixedTypeDisplay(ITypeSymbol nestedType, string suffix)
    {
        var left = CreateSymbolInline(nestedType);
        var suffixRun = Run(suffix, CommonStyles.RawValueBrush);
        return new ComplexGroupedRunInline.Builder([new(left), suffixRun]);
    }
}
