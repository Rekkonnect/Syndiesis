namespace Syndiesis.Tests;

/// <summary>
/// An abstract representation of a class supporting caching the contents of
/// a file from a <see cref="FileInfo"/> source.
/// </summary>
/// <typeparam name="TSelf">The type implementing this interface.</typeparam>
public interface ICacheableFileContents<TSelf>
    where TSelf : ICacheableFileContents<TSelf>
{
    public FileInfo Source { get; }

    public void Clear();

    public static abstract TSelf Create(FileInfo source);
}
