using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.Generic;

namespace CSharpSyntaxEditor.Controls;

public partial class LoadingSpinner : UserControl
{
    public LoadingSpinner()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        InitializeImage();
    }

    private void InitializeImage()
    {
        var spinner = App.CurrentResourceManager.SpinnerImage;
        if (spinner is not Image image)
        {
            throw new KeyNotFoundException("The spinner image was not found in the resources");
        }

        Content = image.CopyOfSource();
    }
}

public static class ImageControlExtensions
{
    public static Image CopyOfSource(this Image image)
    {
        return new() { Source = image.Source };
    }
}
