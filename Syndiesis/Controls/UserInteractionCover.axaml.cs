using Avalonia.Controls;
using Avalonia.Media;

namespace Syndiesis.Controls;

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

    public static class Styling
    {
        public static readonly Color GoodTextColor = Color.FromUInt32(0xFF009099);
        public static readonly Color BadTextColor = Color.FromUInt32(0xFF990045);

        public static readonly SolidColorBrush GoodTextBrush = new(GoodTextColor);
        public static readonly SolidColorBrush BadTextBrush = new(BadTextColor);
    }
}
