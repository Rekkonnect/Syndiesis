using Avalonia.Controls;
using Avalonia.Media;

namespace Syndiesis;

public class AppResourceManager(App app)
{
    private readonly App _app = app;

    public Image? PenImage => _app.FindResource("PenImage") as Image;
    public Image? SpinnerImage => _app.FindResource("SpinnerImage") as Image;
    public Image? SuccessImage => _app.FindResource("SuccessImage") as Image;
    public Image? FailureImage => _app.FindResource("FailureImage") as Image;

    // with help from
    // https://github.com/FrankenApps/Avalonia-CustomTitleBarTemplate
    private const string _minimizeIconPathData =
        """
        M2048 1229v-205h-2048v205h2048z
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
}
