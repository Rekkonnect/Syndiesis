using Avalonia.Controls;
using Avalonia.Media;

namespace CSharpSyntaxEditor.Controls;

public partial class UserInteractionCover : UserControl
{
    public string? DisplayText
    {
        get => textDisplay.Text;
        set => textDisplay.Text = value;
    }

    public object? IconDisplay
    {
        get => iconContainer.Content;
        set => iconContainer.Content = value;
    }

    public IBrush? TextBrush
    {
        get => textDisplay.Foreground;
        set => textDisplay.Foreground = value;
    }

    public UserInteractionCover()
    {
        InitializeComponent();
    }
}
