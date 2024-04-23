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
        var spinner = TopLevel.GetTopLevel(this)!.FindResource("SpinnerImage");
        if (spinner is not Image { Source: var imageSource })
        {
            throw new KeyNotFoundException("The spinner image was not found in the resources");
        }

        Content = new Image { Source = imageSource };
    }
}
