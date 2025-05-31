using Microsoft.CodeAnalysis;
using Serilog;
using Syndiesis.Controls.Inlines;
using Syndiesis.Core;
using Syndiesis.Core.DisplayAnalysis;

namespace Syndiesis.Controls.Editor.QuickInfo;

using ARun = UIBuilder.Run;

// Creating a common creator for the time being,
// since we cannot show the defined value of a preprocessing symbol in VB
// REFERENCE: https://github.com/dotnet/roslyn/issues/76643
// If the above is resolved, the creator can be separated into a C# and a VB one
// offering the initially-designed behavior of showing the defined value of the VB
// symbol alongside its status and the location at which the popup is triggered
public sealed class CommonPreprocessingSymbolExtraInlinesCreator(
    BaseSymbolExtraInlinesCreatorContainer parentContainer)
    : BaseSymbolExtraInlinesCreator<IPreprocessingSymbol>(parentContainer)
{
    public override GroupedRunInline.IBuilder CreateSymbolInline(IPreprocessingSymbol symbol)
    {
        throw new UnreachableException(ExceptionReasons.Liskov);
    }

    protected override void CreateWithHoverContext(
        SymbolHoverContext context, ComplexGroupedRunInline.Builder inlines)
    {
        var tree = context.SemanticModel.SyntaxTree;
        var position = context.TextPosition;
        var node = tree.SyntaxNodeAtPosition(position);
        if (node is null)
        {
            Log.Warning("Failed to get preprocessing symbol node for quick info");
            return;
        }
        var info = context.SemanticModel.GetPreprocessingSymbolInfo(node);
        int line = tree.GetLinePosition(position).Line;

        var definedRun = DefinedRun(info.IsDefined);
        var restRun = Run(
            $" here at line {line}",
            ColorizationStyles.PreprocessingStatementBrush);

        var inline = new SimpleGroupedRunInline.Builder([definedRun, restRun]);
        inlines.Add(inline);
    }

    private static readonly ARun _definedRun = DefinedRun();
    private static readonly ARun _undefinedRun = UndefinedRun();

    private static ARun DefinedRun(bool defined)
    {
        return defined ? _definedRun : _undefinedRun;
    }

    private static ARun DefinedRun()
    {
        return Run("Defined", CommonStyles.RawValueBrush);
    }

    private static ARun UndefinedRun()
    {
        return Run("Undefined", ColorizationStyles.PreprocessingStatementBrush);
    }
}
