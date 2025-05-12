using System.Collections.Immutable;

namespace Syndiesis.Tests;

public static class TestSources
{
    public static readonly ProjectSourceProvider SourceProvider
        = Syndiesis.ProjectSourceProviderGetter.Get();

    public static readonly ProjectSourceProvider TestSourceProvider
        = Syndiesis.Tests.ProjectSourceProviderGetter.Get();

    public static readonly ImmutableArray<FileInfo> MainFilesToTest
        = SourceProvider.GetFilePaths();

    public static readonly ImmutableArray<FileInfo> TestFilesToTest
        = TestSourceProvider.GetFilePaths();

    public static readonly ImmutableArray<FileInfo> FilesToTest =
    [
        .. MainFilesToTest,
        .. TestFilesToTest,
    ];
}
