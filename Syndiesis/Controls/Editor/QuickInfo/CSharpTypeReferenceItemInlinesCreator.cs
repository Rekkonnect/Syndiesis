using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis;
using Serilog;
using Syndiesis.ColorHelpers;
using Syndiesis.Controls.Inlines;
using Syndiesis.Core;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpTypeReferenceItemInlinesCreator(
    CSharpSymbolItemInlinesCreatorContainer parentContainer)
    : BaseCSharpSymbolItemInlinesCreator<ITypeSymbol>(parentContainer)
{
    protected override void AddModifierInlines(ITypeSymbol symbol, GroupedRunInlineCollection inlines)
    {
    }

    protected override GroupedRunInline.IBuilder CreateSymbolInline(ITypeSymbol symbol)
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
        }
        
        // We must never leave any symbol kinds unimplemented
        Log.Warning("Found unimplemented symbol type kind {typeKind}", type.TypeKind);
        return new SimpleGroupedRunInline.Builder();
    }

    public GroupedRunInline.IBuilder CreateNamedTypeInline(INamedTypeSymbol type)
    {
        if (type.IsTupleType)
        {
            return CreateTupleInline(type);
        }
        
        if (type.IsAnonymousType)
        {
            return CreateAnonymousInline(type);
        }

        return CreateOrdinaryNamedTypeInline(type);
    }

    public GroupedRunInline.IBuilder CreateTypeParameterInline(ITypeParameterSymbol type)
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
        
        var run = Run(type.Name, ColorizationStyles.TypeParameterBrush);
        inlines.AddChild(new SingleRunInline.Builder(run));
        return inlines;

        void AddVariance(string keyword)
        {
            var varianceRun = Run(keyword, CommonStyles.KeywordBrush);
            inlines.AddChild(varianceRun);
        }
    }
    
    public GroupedRunInline.IBuilder CreateFunctionPointerInline(IFunctionPointerTypeSymbol type)
    {
        var signature = type.Signature;

        var inlines = new ComplexGroupedRunInline.Builder();
            
        var delegateRun = Run("delegate", CommonStyles.KeywordBrush);
        inlines.AddChild(delegateRun);

        var asteriskRun = Run("*", CommonStyles.RawValueBrush);
        inlines.AddChild(asteriskRun);
        if (signature.CallingConvention is SignatureCallingConvention.Unmanaged)
        {
            var unmanagedRun = Run(" unmanaged", CommonStyles.KeywordBrush);
            inlines.AddChild(unmanagedRun);
            
            var callingConventionTypes = signature.UnmanagedCallingConventionTypes;
            if (callingConventionTypes.Length > 0)
            {
                var callingConventionRuns = new List<RunOrGrouped>([]);
                var callingConventionListStart = Run("[", CommonStyles.RawValueBrush);
                callingConventionRuns.Add(callingConventionListStart);
                
                for (var i = 0; i < callingConventionTypes.Length; i++)
                {
                    var convention = callingConventionTypes[i];
                    var conventionName = DisplayForUnmanagedCallingConventionType(convention);
                    // Hard-code the class brush since the calling convention types are all classes
                    var run = Run(conventionName, CommonStyles.ClassMainBrush);
                    var conventionInline = new SingleRunInline.Builder(run);
                    callingConventionRuns.Add(new RunOrGrouped(conventionInline));

                    if (i < callingConventionTypes.Length - 1)
                    {
                        var separator = CreateArgumentSeparatorRun();
                        callingConventionRuns.Add(separator);
                    }
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

    private static string DisplayForUnmanagedCallingConventionType(INamedTypeSymbol type)
    {
        // Assuming that the name is CallConvXyz, for example CallConvCdecl
        // TODO: Test this
        const string prefix = "CallConv";
        int prefixLength = prefix.Length;
        return type.Name[prefixLength..];
    }
    
    public GroupedRunInline.IBuilder CreateOrdinaryNamedTypeInline(INamedTypeSymbol type)
    {
        var typeBrush = RoslynColorizationHelpers.BrushForTypeKind(ColorizationStyles, type.TypeKind)!;
        
        if (type.IsGenericType)
        {
            var name = type.GetNameWithoutGenericSuffix();

            var inlines = new ComplexGroupedRunInline.Builder();
            
            var nameRun = Run(name.ToString(), typeBrush);
            inlines.AddChild(nameRun);
            var openingTag = Run($"<", CommonStyles.RawValueBrush);
            inlines.AddChild(openingTag);
            var arguments = type.TypeArguments;
            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = arguments[i];
                var inner = CreateTypeInline(argument);
                inlines.AddChild(inner);

                if (i < arguments.Length - 1)
                {
                    var separator = CreateArgumentSeparatorRun();
                    inlines.AddChild(separator);
                }
            }

            var closingTag = Run(">", CommonStyles.RawValueBrush);
            inlines.AddChild(closingTag);

            return inlines;
        }
        
        var symbolRun = Run(type.Name, typeBrush);
        return new ComplexGroupedRunInline.Builder([symbolRun]);
    }
    
    public GroupedRunInline.IBuilder CreateTupleInline(INamedTypeSymbol tupleType)
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
    
    public GroupedRunInline.IBuilder CreateAnonymousInline(INamedTypeSymbol anonymousType)
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