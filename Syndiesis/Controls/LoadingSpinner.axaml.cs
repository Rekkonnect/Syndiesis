namespace Syndiesis.Controls;

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
