using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;

namespace Syndiesis;

public sealed class ProjectSourceProvider(string? callerFilePath)
{
    private readonly string? _callerFilePath = callerFilePath;
    
    public ImmutableArray<FileInfo> GetFilePaths()
    {
        if (string.IsNullOrEmpty(_callerFilePath))
            return [];

        var parentDirectory = Directory.GetParent(_callerFilePath);
        if (parentDirectory is null)
            return [];

        var files = parentDirectory.GetFiles("*.cs", SearchOption.AllDirectories);
        return [.. files];
    }

    private static string? ThisPath([CallerFilePath] string? callerFilePath = null)
    {
        return callerFilePath;
    }

    public static ProjectSourceProvider Get()
    {
        var thisPath = ThisPath();
        return new(thisPath);
    }
}
