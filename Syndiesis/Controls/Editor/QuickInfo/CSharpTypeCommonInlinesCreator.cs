using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using RoseLynn.CSharp.Syntax;
using Serilog;
using SkiaSharp;
using Syndiesis.ColorHelpers;
using Syndiesis.Controls.Inlines;
using Syndiesis.Core;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection.Metadata;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpTypeCommonInlinesCreator(
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

        Log.Warning("Found unimplemented fallback symbol type kind {typeKind}", type.TypeKind);
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
        inlines.AddChild(run);
        return inlines;

        void AddVariance(string keyword)
        {
            var varianceRun = Run(keyword, CommonStyles.KeywordBrush);
            inlines.AddChild(varianceRun);
        }
    }

    private GroupedRunInline.IBuilder CreateFunctionPointerInline(IFunctionPointerTypeSymbol type)
    {
        var signature = type.Signature;

        var inlines = new ComplexGroupedRunInline.Builder();

        var delegateRun = Run("delegate", CommonStyles.KeywordBrush);
        inlines.AddChild(delegateRun);

        var asteriskRun = Run("*", CommonStyles.RawValueBrush);
        inlines.AddChild(asteriskRun);
        if (signature.CallingConvention is not SignatureCallingConvention.Default)
        {
            var unmanagedRun = Run(" unmanaged", CommonStyles.KeywordBrush);
            inlines.AddChild(unmanagedRun);

            var callingConventionNames = GetConventionNameList(signature);
            if (callingConventionNames.Length > 0)
            {
                var callingConventionRuns = new List<RunOrGrouped>([]);
                var callingConventionListStart = Run("[", CommonStyles.RawValueBrush);
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
                    callingConventionRuns.Add(new RunOrGrouped(conventionInline));
                }

                var callingConventionListEnd = Run("]", CommonStyles.RawValueBrush);
                callingConventionRuns.Add(callingConventionListEnd);

                var callingConventionInline = new ComplexGroupedRunInline.Builder(callingConventionRuns);
                inlines.AddChild((GroupedRunInline.IBuilder)callingConventionInline);
            }
        }

        var openingTag = Run("<", CommonStyles.RawValueBrush);
        inlines.AddChild(openingTag);
        var parameters = signature.Parameters;
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            var inner = CreateTypeInline(parameter.Type);
            inlines.AddChild(inner);

            var separator = CreateArgumentSeparatorRun();
            inlines.AddChild(separator);
        }

        var returnTypeInline = CreateTypeInline(signature.ReturnType);
        inlines.AddChild(returnTypeInline);

        var closingTag = Run(">", CommonStyles.RawValueBrush);
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
            SignatureCallingConvention.ThisCall => "Thiscall",
            SignatureCallingConvention.FastCall => "Fastcall",
            SignatureCallingConvention.StdCall => "Stdcall",
            SignatureCallingConvention.CDecl => "Cdecl",
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
        var typeBrush = RoslynColorizationHelpers.BrushForTypeKind(ColorizationStyles, type.TypeKind)!;
        var symbolRun = SingleRun(type.Name, typeBrush);

        if (!type.IsGenericType)
        {
            return symbolRun;
        }

        var inlines = new ComplexGroupedRunInline.Builder();

        inlines.AddChild(symbolRun);
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
        var left = CreateTypeInline(field.Type);

        var fieldBrush = ColorizationStyles.FieldBrush;
        ILazilyUpdatedBrush brush = fieldBrush;
        if (!field.IsExplicitlyNamedTupleElement)
        {
            var fadeTransformation = new HsvTransformation(0, 0, 0, 0.4);
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
        var left = CreateTypeInline(property.Type);
        var nameRun = Run(property.Name, CommonStyles.PropertyBrush);
        var suffixGroup = new SimpleGroupedRunInline.Builder([nameRun]);
        return new ComplexGroupedRunInline.Builder([new(left), suffixGroup]);
    }

    private GroupedRunInline.IBuilder SuffixedTypeDisplay(ITypeSymbol nestedType, string suffix)
    {
        var left = CreateTypeInline(nestedType);
        var suffixRun = Run(suffix, CommonStyles.RawValueBrush);
        var suffixGroup = new SimpleGroupedRunInline.Builder([suffixRun]);
        return new ComplexGroupedRunInline.Builder([new(left), suffixGroup]);
    }
}
