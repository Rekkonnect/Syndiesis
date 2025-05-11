using Avalonia.Media;
using Garyon.Reflection;
using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Editor;
using Syndiesis.Controls.Inlines;
using System;

namespace Syndiesis.Core.DisplayAnalysis;

using AnalysisTreeListNode = UIBuilder.AnalysisTreeListNode;
using AnalysisTreeListNodeLine = UIBuilder.AnalysisTreeListNodeLine;
using ComplexGroupedRunInline = ComplexGroupedRunInline.Builder;
using GroupedRunInline = GroupedRunInline.IBuilder;
using Run = UIBuilder.Run;
using SimpleGroupedRunInline = SimpleGroupedRunInline.Builder;
using SingleRunInline = SingleRunInline.Builder;

public abstract class BaseInlineCreator
{
    public static BaseAnalysisNodeCreator.NodeCommonStyles CommonStyles
        => AppSettings.Instance.NodeColorPreferences.CommonStyles!;

    public static RoslynColorizer.ColorizationStyles ColorizationStyles
        => AppSettings.Instance.ColorizationPreferences.ColorizationStyles!;

    protected static SingleRunInline SingleRun(string text, ILazilyUpdatedBrush brush)
    {
        return new(Run(text, brush));
    }
    
    protected static Run Run(string text, ILazilyUpdatedBrush brush)
    {
        return new(text, brush);
    }

    protected static Run Run(string text, ILazilyUpdatedBrush brush, FontStyle fontStyle)
    {
        return new(
            text,
            brush,
            fontStyle);
    }

    protected static Run KeywordRun(string text)
    {
        return new(text, CommonStyles.KeywordBrush);
    }

    protected static SingleRunInline SingleKeywordRun(string text)
    {
        return new(KeywordRun(text));
    }

    protected static void AddKeywordRun(string text, GroupedRunInlineCollection inlines)
    {
        inlines.Add(KeywordRun(text));
    }
    
    protected static void AddKeywordRun(string text, ComplexGroupedRunInline inlines)
    {
        inlines.AddChild(KeywordRun(text));
    }

    protected static void AddKeywordAndSpaceRun(string text, GroupedRunInlineCollection inlines)
    {
        AddKeywordRun(text, inlines);
        var space = CreateSpaceSeparatorRun();
        inlines.Add(space);
    }

    protected static void AddKeywordAndSpaceRun(string text, ComplexGroupedRunInline inlines)
    {
        AddKeywordRun(text, inlines);
        var space = CreateSpaceSeparatorRun();
        inlines.AddChild(space);
    }

    protected static Run CreateArgumentSeparatorRun()
    {
        return Run(", ", CommonStyles.RawValueBrush);
    }

    protected static Run CreateQualifierSeparatorRun()
    {
        return Run(".", CommonStyles.RawValueBrush);
    }

    protected static Run CreateSpaceSeparatorRun()
    {
        return Run(" ", CommonStyles.RawValueBrush);
    }

    protected static string CreateArrayRankDisplay(int rank)
    {
        int repeats = rank - 1;
        var commas = new string(',', repeats);
        return $"[{commas}]";
    }

    protected static ILazilyUpdatedBrush GetFieldBrush(IFieldSymbol field)
    {
        if (field.IsConst)
            return CommonStyles.ConstantMainBrush;

        if (field.IsEnumField())
            return CommonStyles.EnumFieldMainBrush;

        return ColorizationStyles.FieldBrush;
    }
}
