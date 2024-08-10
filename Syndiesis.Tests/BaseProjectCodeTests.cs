using System.Collections.Immutable;

[assembly: Parallelizable(ParallelScope.Fixtures)]

namespace Syndiesis.Tests;

public abstract class BaseProjectCodeTests
{
    protected static readonly ProjectSourceProvider SourceProvider
        = ProjectSourceProvider.Get();

    protected static readonly ImmutableArray<FileInfo> FilesToTest
        = SourceProvider.GetFilePaths();

    [Test]
    public async Task TestAllFilesIndependently()
    {
        Assert.That(FilesToTest, Is.Not.Empty);

        await Parallel.ForEachAsync(
            FilesToTest,
            TestFile);

        async ValueTask TestFile(FileInfo file, CancellationToken cancellationToken)
        {
            var text = await File.ReadAllTextAsync(file.FullName, cancellationToken);
            await TestSource(text);
        }
    }

    protected abstract Task TestSource(string text);
}
