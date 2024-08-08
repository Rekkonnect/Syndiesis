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

        var sourceTests = new List<Task>();
        foreach (var file in FilesToTest)
        {
            var text = await File.ReadAllTextAsync(file.FullName);
            var testTask = TestSource(text);
            sourceTests.Add(testTask);
        }

        await Task.WhenAll(sourceTests);
    }

    protected abstract Task TestSource(string text);
}
