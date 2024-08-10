using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;

namespace Syndiesis;

public class ProjectSourceProvider(string? callerFilePath)
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

    public static string? CallerFilePath([CallerFilePath] string? callerFilePath = null)
    {
        return callerFilePath;
    }
}
