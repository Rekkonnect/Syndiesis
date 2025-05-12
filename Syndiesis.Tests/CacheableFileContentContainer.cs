using Garyon.Objects;
using System.Collections.Concurrent;

namespace Syndiesis.Tests;

/// <summary>
/// Contains instances of <typeparamref name="TContent"/> mapped by the full
/// file name of the source <see cref="FileInfo"/>. Ensures that each unique
/// file instance only has at most one <see cref="TContent"/> instance mapped
/// to it, lazily creating them.
/// </summary>
public sealed class CacheableFileContentContainer<TContent>
    where TContent : class, ICacheableFileContents<TContent>
{
    private readonly ConcurrentDictionary<string, TContent> _files = new();

    public static CacheableFileContentContainer<TContent> Shared
        => Singleton<CacheableFileContentContainer<TContent>>.Instance;

    public TContent Get(string path)
    {
        return Get(new FileInfo(path));
    }

    public TContent Get(FileInfo file)
    {
        bool contained = _files.TryGetValue(file.FullName, out var cached);
        if (!contained)
        {
            cached = TContent.Create(file);
            _files[file.FullName] = cached;
        }
        return cached!;
    }

    /// <summary>
    /// Sets or adds a cached content instance to this container.
    /// If the file name already exists in the container, its cached contents
    /// instance is overwritten.
    /// </summary>
    public void Set(TContent content)
    {
        _files[content.Source.FullName] = content;
    }
}
