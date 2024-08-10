namespace Syndiesis;

public static class ProjectSourceProviderGetter
{
    public static ProjectSourceProvider Get()
    {
        var thisPath = ProjectSourceProvider.CallerFilePath();
        return new(thisPath);
    }
}
