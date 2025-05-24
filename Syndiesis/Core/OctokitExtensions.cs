using Octokit;
using System.Collections.Generic;
using System.Linq;

namespace Syndiesis.Core;

public static class OctokitExtensions
{
    public static string ShortCommitSha(this string sha, int digits = 7)
    {
        return sha[..digits];
    }

    public static async Task<RepositoryTag> GetTagFromRelease(
        this IGitHubClient client, Release release, string owner, string repository)
    {
        var tagName = release.TagName;
        
        var tags = await client.Repository.GetAllTags(
            owner, repository);

        var tag = tags.FirstOrDefault(s => s.Name == tagName);
        if (tag is null)
        {
            throw new KeyNotFoundException(
                "The repository did not contain a tag with that name");
        }
        return tag;
    }
}
