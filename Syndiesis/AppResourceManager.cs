using Avalonia.Controls;
using Avalonia.Media;

namespace Syndiesis;

public class AppResourceManager(App app)
{
    private readonly App _app = app;

    public Image? LogoCSImage => ImageResource("LogoCSImage");
    public Image? LogoVBImage => ImageResource("LogoVBImage");
    public Image? PenImage => ImageResource("PenImage");
    public Image? SpinnerImage => ImageResource("SpinnerImage");
    public Image? SuccessImage => ImageResource("SuccessImage");
    public Image? FailureImage => ImageResource("FailureImage");

    public Image? DiagnosticErrorImage => ImageResource("DiagnosticErrorImage");
    public Image? DiagnosticWarningImage => ImageResource("DiagnosticWarningImage");
    public Image? DiagnosticSuggestionImage => ImageResource("DiagnosticSuggestionImage");

    // Symbol icons
    public Image? ClassImage => ImageResource("VisualStudio_Symbol_Class");
    public Image? ConstantImage => ImageResource("VisualStudio_Symbol_Constant");
    public Image? DelegateImage => ImageResource("VisualStudio_Symbol_Delegate");
    public Image? EnumImage => ImageResource("VisualStudio_Symbol_Enum");
    public Image? EnumFieldImage => ImageResource("VisualStudio_Symbol_EnumField");
    public Image? EventImage => ImageResource("VisualStudio_Symbol_Event");
    public Image? InterfaceImage => ImageResource("VisualStudio_Symbol_Interface");
    public Image? LocalImage => ImageResource("VisualStudio_Symbol_Local");
    public Image? MethodImage => ImageResource("VisualStudio_Symbol_Method");
    public Image? ModuleImage => ImageResource("VisualStudio_Symbol_Module");
    public Image? NamespaceImage => ImageResource("VisualStudio_Symbol_Namespace");
    public Image? ParamImage => ImageResource("VisualStudio_Symbol_Param");
    public Image? PropImage => ImageResource("VisualStudio_Symbol_Prop");
    public Image? StructImage => ImageResource("VisualStudio_Symbol_Struct");
    public Image? TypeParamImage => ImageResource("VisualStudio_Symbol_TypeParam");
    public Image? LabelImage => ImageResource("VisualStudio_Symbol_Label");
    public Image? FieldImage => ImageResource("VisualStudio_Symbol_Field");
    public Image? AssemblyImage => ImageResource("VisualStudio_Symbol_Assembly");
    
    // with help from
    // https://github.com/FrankenApps/Avalonia-CustomTitleBarTemplate
    private const string _minimizeIconPathData =
        """
        M0 0h2048M2048 1229v-205h-2048v205h2048z
        """;

    private const string _restoreDownIconPathData =
        """
        M2048 1638h-410v410h-1638v-1638h410v-410h1638v1638zm-614-1024h-1229v1229h1229v-1229zm409-409h-1229v205h1024v1024h205v-1229z
        """;

    private const string _maximizeIconPathData =
        """
        M2048 2048v-2048h-2048v2048h2048zM1843 1843h-1638v-1638h1638v1638z
        """;

    private const string _closeIconPathData =
        """
        M1169 1024l879 -879l-145 -145l-879 879l-879 -879l-145 145l879 879l-879 879l145 145l879 -879l879 879l145 -145z
        """;

    public Geometry MinimizeIconGeometry { get; } = Geometry.Parse(_minimizeIconPathData);
    public Geometry RestoreDownIconGeometry { get; } = Geometry.Parse(_restoreDownIconPathData);
    public Geometry MaximizeIconGeometry { get; } = Geometry.Parse(_maximizeIconPathData);
    public Geometry CloseIconGeometry { get; } = Geometry.Parse(_closeIconPathData);

    private Image? ImageResource(string key)
    {
        return _app.FindResource(key) as Image;
    }
}
