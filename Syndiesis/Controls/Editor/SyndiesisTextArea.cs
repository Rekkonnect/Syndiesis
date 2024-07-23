using Avalonia.Interactivity;
using AvaloniaEdit.Editing;

namespace Syndiesis.Controls;

public sealed class SyndiesisTextArea : TextArea
{
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Caret.Show();
    }

    // Preserve the caret even when losing focus
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        Caret.Show();
    }
}
