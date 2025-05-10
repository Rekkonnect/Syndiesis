using Avalonia.Controls;
using Garyon.Objects;
using Microsoft.CodeAnalysis;
using Syndiesis.Controls.Inlines;
using Syndiesis.Core;
using System;

namespace Syndiesis.Controls.Editor.QuickInfo;

public partial class QuickInfoSymbolItem : UserControl
{
    public HybridSingleTreeCompilationSource? CompilationSource { get; set; }
    
    public QuickInfoSymbolItem()
    {
        InitializeComponent();
    }

    public void LoadSymbol(ISymbol symbol)
    {
        var image = ImageForSymbol(symbol);
        var documentationRoot = XmlDocumentationAnalysisRoot.CreateForSymbol(symbol);
        symbolIcon.Source = image.Source;
        symbolDisplayBlock.GroupedRunInlines = CreateGroupedRunForSymbol(symbol);
        documentationDisplayBlock.GroupedRunInlines = CreateGroupedRunForSymbolDocumentation(
            documentationRoot);
        documentationDisplayBlock.IsVisible = documentationDisplayBlock.GroupedRunInlines is not null;
    }

    private GroupedRunInlineCollection? CreateGroupedRunForSymbolDocumentation(
        XmlDocumentationAnalysisRoot? documentationAnalysisRoot)
    {
        // TODO: Pass this through a grouped run creator for the contents of the overview
        return null;
    }

    private GroupedRunInlineCollection CreateGroupedRunForSymbol(ISymbol symbol)
    {
        var language = CompilationSource?.CurrentLanguageName ?? LanguageNames.CSharp;
        var hybridContainer = Singleton<HybridLanguageSymbolItemInlinesCreatorContainer>.Instance;
        var container = hybridContainer.ContainerForLanguage(language);
        var groupedRun = new GroupedRunInlineCollection();
        container.Definitions.CreatorForSymbol(symbol).Create(symbol, groupedRun);
        return groupedRun;
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
            
            QuickInfoSymbolClassification.Field => resources.FieldImage,
            QuickInfoSymbolClassification.Property => resources.PropImage,
            QuickInfoSymbolClassification.Event => resources.EventImage,
            QuickInfoSymbolClassification.Method => resources.MethodImage,
            QuickInfoSymbolClassification.Operator => resources.OperatorImage,
            // TODO: Provide an icon specifically for the conversion
            QuickInfoSymbolClassification.Conversion => resources.OperatorImage,
            QuickInfoSymbolClassification.EnumField => resources.EnumFieldImage,
            
            QuickInfoSymbolClassification.Label => resources.LabelImage,
            QuickInfoSymbolClassification.Local => resources.LocalImage,
            QuickInfoSymbolClassification.Parameter => resources.ParamImage,
            QuickInfoSymbolClassification.Constant => resources.ConstantImage,
            QuickInfoSymbolClassification.TypeParameter => resources.TypeParamImage,
            // TODO: Provide an icon for preprocessing symbols
            QuickInfoSymbolClassification.Preprocessing => resources.LabelImage,

            _ => throw new NotSupportedException("The symbol is not correctly classified"),
        };
    }

    private QuickInfoSymbolItem LoadSymbolWithCompilationSource(
        ISymbol symbol, HybridSingleTreeCompilationSource? compilationSource)
    {
        CompilationSource = compilationSource;
        LoadSymbol(symbol);
        return this;
    }

    public static QuickInfoSymbolItem CreateForSymbolAndCompilation(
        ISymbol symbol, HybridSingleTreeCompilationSource? compilationSource)
    {
        return new QuickInfoSymbolItem()
            .LoadSymbolWithCompilationSource(symbol, compilationSource);
    }
}