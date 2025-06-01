namespace Syndiesis.Controls;

public static class ImageControlExtensions
{
    public static Image CopyOfSource(this Image image)
    {
        return new() { Source = image.Source };
    }
}
