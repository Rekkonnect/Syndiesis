using Avalonia.Media;
using Syndiesis.Controls.Inlines;

namespace Syndiesis.Core.DisplayAnalysis;

using Run = UIBuilder.Run;
using AnalysisTreeListNode = UIBuilder.AnalysisTreeListNode;
using AnalysisTreeListNodeLine = UIBuilder.AnalysisTreeListNodeLine;

using GroupedRunInline = GroupedRunInline.IBuilder;
using SingleRunInline = SingleRunInline.Builder;
using SimpleGroupedRunInline = SimpleGroupedRunInline.Builder;
using ComplexGroupedRunInline = ComplexGroupedRunInline.Builder;

public abstract class BaseInlineCreator
{
    public static BaseAnalysisNodeCreator.NodeCommonStyles CommonStyles
        => AppSettings.Instance.NodeColorPreferences.CommonStyles!;

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

    protected static void AddKeywordRun(string text, GroupedRunInlineCollection inlines)
    {
        inlines.Add(KeywordRun(text));
    }
    
    protected static void AddKeywordRun(string text, ComplexGroupedRunInline inlines)
    {
        inlines.AddChild(KeywordRun(text));
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
}
