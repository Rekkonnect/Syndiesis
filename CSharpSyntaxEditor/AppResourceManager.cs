using Avalonia.Controls;

namespace CSharpSyntaxEditor;

public class AppResourceManager(App app)
{
    private readonly App _app = app;

    public Image? PenImage => _app.FindResource("PenImage") as Image;
    public Image? SpinnerImage => _app.FindResource("SpinnerImage") as Image;
    public Image? SuccessImage => _app.FindResource("SuccessImage") as Image;
}
