using Syndiesis.Core;

namespace Syndiesis.Utilities;

public sealed record class CommitSha(string Full)
{
    public string Short { get; } = Full.ShortCommitSha();

    public static implicit operator CommitSha(string full) => new(full);

    public static CommitSha? FromString(string? sha)
    {
        if (sha is null)
            return null;

        return new(sha);
    }
}
