using Garyon.Objects;
using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;
using Syndiesis.Core;

namespace Syndiesis.Controls.Editor.QuickInfo;

public partial class QuickInfoSymbolItem : UserControl
{
    public HybridSingleTreeCompilationSource? CompilationSource { get; set; }

    public QuickInfoSymbolItem()
    {
        InitializeComponent();
    }

    public void LoadSymbol(SymbolHoverContext context)
    {
        var symbol = context.Symbol;
        var image = ImageForSymbol(symbol);
        var container = GetSymbolContainer();
        symbolIcon.Source = image.Source;
        symbolDisplayBlock.GroupedRunInlines = CreateSymbolDefinitionGroupedRun(container, context);

        SetContent(
            extrasDisplayBlock,
            CreateSymbolExtrasGroupedRun(container, context));

        var documentationRoot = XmlDocumentationAnalysisRoot.CreateForSymbol(symbol);
        SetContent(
            documentationDisplayBlock,
            CreateSymbolDocumentationGroupedRun(container, documentationRoot));
    }

    private static void SetContent(
        CopyableGroupedRunInlineTextBlock block,
        GroupedRunInlineCollection? inlines)
    {
        block.GroupedRunInlines = inlines;
        SetVisibleOnNonEmptyContent(block);
    }

    private static void SetVisibleOnNonEmptyContent(CopyableGroupedRunInlineTextBlock block)
    {
        bool isEmpty = block.GroupedRunInlines is null or [];
        block.IsVisible = !isEmpty;
    }

    private GroupedRunInlineCollection? CreateSymbolDocumentationGroupedRun(
        ISymbolInlinesRootCreatorContainer container, XmlDocumentationAnalysisRoot? documentationAnalysisRoot)
    {
        if (documentationAnalysisRoot is null)
        {
            return null;
        }

        var symbol = documentationAnalysisRoot.Symbol;
        var groupedRun = new ComplexGroupedRunInline.Builder();
        container.Docs.CreatorForSymbol(symbol)?.Create(symbol, groupedRun);
        return [groupedRun];
    }

    private GroupedRunInlineCollection? CreateSymbolExtrasGroupedRun(
        ISymbolInlinesRootCreatorContainer container, SymbolHoverContext context)
    {
        var inlines = container.Extras.CreatorForSymbol(context.Symbol)
            ?.Create(context);

        // TODO: This could be approached more generically by recursively
        // iterating all the inlines and groups until a non-empty one is found
        var hasEmptyContent = inlines is null or { HasAny: false };
        if (hasEmptyContent)
            return null;

        return [inlines];
    }

    private GroupedRunInlineCollection CreateSymbolDefinitionGroupedRun(
        ISymbolInlinesRootCreatorContainer container, SymbolHoverContext context)
    {
        var groupedRun = new ComplexGroupedRunInline.Builder();
        container.Definitions.CreatorForSymbol(context.Symbol)
            .CreateWithHoverContext(context, groupedRun);
        return [groupedRun];
    }

    private ISymbolInlinesRootCreatorContainer GetSymbolContainer()
    {
        var language = CompilationSource?.CurrentLanguageName ?? LanguageNames.CSharp;
        var hybridContainer = Singleton<HybridLanguageSymbolItemInlinesCreatorContainer>.Instance;
        return hybridContainer.ContainerForLanguage(language);
    }

    private static Image ImageForSymbol(ISymbol symbol)
    {
        var classification = QuickInfoSymbolClassifier.ClassifySymbol(symbol);
        return ImageForSymbolClassification(classification)!;
    }

    private static Image? ImageForSymbolClassification(QuickInfoSymbolClassification classification)
    {
        var resources = App.CurrentResourceManager;
        return classification switch
        {
            QuickInfoSymbolClassification.Namespace => resources.NamespaceImage,
            QuickInfoSymbolClassification.Alias => resources.LabelImage,
            QuickInfoSymbolClassification.Module => resources.ModuleImage,
            QuickInfoSymbolClassification.Assembly => resources.AssemblyImage,

            QuickInfoSymbolClassification.Class => resources.ClassImage,
            QuickInfoSymbolClassification.Struct => resources.StructImage,
            QuickInfoSymbolClassification.Interface => resources.InterfaceImage,
            QuickInfoSymbolClassification.Enum => resources.EnumImage,
            QuickInfoSymbolClassification.Delegate => resources.DelegateImage,
            // TODO: Provide an icon denoting the erroneous symbol
            QuickInfoSymbolClassification.Error => resources.LabelImage,
            QuickInfoSymbolClassification.Dynamic => resources.ClassImage,
            QuickInfoSymbolClassification.Array => resources.ClassImage,
            QuickInfoSymbolClassification.Pointer => resources.StructImage,
            QuickInfoSymbolClassification.FunctionPointer => resources.DelegateImage,
            QuickInfoSymbolClassification.Extension => resources.ClassImage,

            QuickInfoSymbolClassification.Field => resources.FieldImage,
            QuickInfoSymbolClassification.Property => resources.PropImage,
            QuickInfoSymbolClassification.Event => resources.EventImage,
            QuickInfoSymbolClassification.Method => resources.MethodImage,
            QuickInfoSymbolClassification.Operator => resources.OperatorImage,
            // TODO: Provide an icon specifically for conversions
            QuickInfoSymbolClassification.Conversion => resources.OperatorImage,
            // TODO: Provide an icon specifically for constructors
            QuickInfoSymbolClassification.Constructor => resources.MethodImage,
            QuickInfoSymbolClassification.EnumField => resources.EnumFieldImage,

            QuickInfoSymbolClassification.Label => resources.LabelImage,
            QuickInfoSymbolClassification.Local => resources.LocalImage,
            QuickInfoSymbolClassification.RangeVariable => resources.LocalImage,
            QuickInfoSymbolClassification.Discard => resources.LocalImage,
            QuickInfoSymbolClassification.Parameter => resources.ParamImage,
            QuickInfoSymbolClassification.Constant => resources.ConstantImage,
            QuickInfoSymbolClassification.TypeParameter => resources.TypeParamImage,
            // TODO: Provide an icon for preprocessing symbols
            QuickInfoSymbolClassification.Preprocessing => resources.LabelImage,

            _ => throw new NotSupportedException("The symbol is not correctly classified"),
        };
    }

    private QuickInfoSymbolItem LoadSymbolWithCompilationSource(
        SymbolHoverContext context, HybridSingleTreeCompilationSource? compilationSource)
    {
        CompilationSource = compilationSource;
        LoadSymbol(context);
        return this;
    }

    public static QuickInfoSymbolItem CreateForSymbolAndCompilation(
        SymbolHoverContext context, HybridSingleTreeCompilationSource? compilationSource)
    {
        return new QuickInfoSymbolItem()
            .LoadSymbolWithCompilationSource(context, compilationSource);
    }
}
