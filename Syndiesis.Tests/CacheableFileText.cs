namespace Syndiesis.Tests;

/// <summary>
/// Wraps a <see cref="FileInfo"/> for the source file whose contents to
/// cache in memory. Only reads the file's text upon the first invocation
/// of <see cref="GetTextAsync"/>.
/// </summary>
/// <remarks>
/// Prefer using <see cref="CacheableFileContentContainer{TContent}"/> and
/// retrieving instances of this class from there to store the file contents
/// in the shared instance.
/// <br/>
/// This implementation is thread-safe, locking the entire file reading
/// operation ensuring that only one thread ever performs the read.
/// </remarks>
public sealed class CacheableFileText(FileInfo source)
    : ICacheableFileContents<CacheableFileText>
{
    public FileInfo Source { get; } = source;

    private string? _cachedText = null;

    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

    public async ValueTask<string> GetTextAsync(
        CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        if (cancellationToken.IsCancellationRequested)
        {
            _semaphore.Release();
            return _cachedText!;
        }
        _cachedText ??= await File.ReadAllTextAsync(Source.FullName, cancellationToken);
        _semaphore.Release();

        return _cachedText;
    }

    public void Clear()
    {
        _cachedText = null;
    }

    static CacheableFileText ICacheableFileContents<CacheableFileText>.Create(FileInfo file)
    {
        return new(file);
    }
}
