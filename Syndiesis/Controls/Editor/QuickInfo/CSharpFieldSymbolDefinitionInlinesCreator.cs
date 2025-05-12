using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;
using Syndiesis.Core;

namespace Syndiesis.Controls.Editor.QuickInfo;

public sealed class CSharpFieldSymbolDefinitionInlinesCreator(
    CSharpSymbolDefinitionInlinesCreatorContainer parentContainer)
    : BaseCSharpSymbolDefinitionInlinesCreator<IFieldSymbol>(parentContainer)
{
    public override GroupedRunInline.IBuilder CreateSymbolInline(IFieldSymbol field)
    {
        var inlines = new ComplexGroupedRunInline.Builder();
        CreateInlines(field, inlines);
        return inlines;
    }

    private void CreateInlines(IFieldSymbol field, ComplexGroupedRunInline.Builder inlines)
    {
        if (field.IsEnumField())
        {
            CreateEnumFieldInlines(field, inlines);
        }
        else
        {
            CreateNormalFieldInlines(field, inlines);
        }
    }
    
    private void CreateEnumFieldInlines(IFieldSymbol field, ComplexGroupedRunInline.Builder inlines)
    {
        var nameBrush = GetFieldBrush(field);
        var nameInline = SingleRun(field.Name, nameBrush);
        inlines.Add(nameInline);
        
        AddFieldConstantValue(field, inlines);
    }

    private void AddFieldConstantValue(IFieldSymbol field, ComplexGroupedRunInline.Builder inlines)
    {
        inlines.Add(CreateSpaceSeparatorRun());
        
        inlines.Add(Run("=", CommonStyles.RawValueBrush));

        inlines.Add(CreateSpaceSeparatorRun());
        
        var constInline = CreateConstantValueInline(field.ConstantValue, field.HasConstantValue);
        inlines.Add(constInline);
    }

    private RunOrGrouped CreateConstantValueInline(object? value, bool hasValue)
    {
        if (!hasValue)
        {
            var missingBrush = AppSettings.Instance.NodeColorPreferences.SyntaxStyles!.MissingTokenIndicatorBrush;
            return Run("?",  missingBrush);
        }

        if (value is null)
        {
            return new(SingleKeywordRun("null"));
        }

        var literalBrush = LiteralBrush(value);
        var valueDisplay = ConstantValueDisplay(value);
        return new(SingleRun(valueDisplay, literalBrush));
    }

    private static ILazilyUpdatedBrush LiteralBrush(object? value)
    {
        if (value is string)
        {
            return ColorizationStyles.StringLiteralBrush;
        }
        
        return ColorizationStyles.NumericLiteralBrush;
    }
    
    private static string ConstantValueDisplay(object value)
    {
        if (value is string)
        {
            return $"\"{value}\"";
        }
        
        return value!.ToString()!;
    }

    private void CreateNormalFieldInlines(IFieldSymbol field, ComplexGroupedRunInline.Builder inlines)
    {
        var type = field.Type;
        var typeInline = ParentContainer.RootContainer.Commons.CreatorForSymbol(type).CreateSymbolInline(type);
        inlines.AddChild(typeInline);
        inlines.Add(CreateSpaceSeparatorRun());
        
        var nameBrush = GetFieldBrush(field);
        var nameInline = SingleRun(field.Name, nameBrush);
        inlines.Add(nameInline);

        if (field.HasConstantValue)
        {
            AddFieldConstantValue(field, inlines);
        }
    }
}