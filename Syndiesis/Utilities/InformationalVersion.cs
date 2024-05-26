using Garyon.Extensions;
using System;
using System.Reflection;

namespace Syndiesis.Utilities;

public sealed record InformationalVersion(string Version, string? CommitSha)
{
    public static InformationalVersion Parse(AssemblyInformationalVersionAttribute attribute)
    {
        return Parse(attribute.InformationalVersion);
    }

    public static InformationalVersion Parse(string versionString)
    {
        bool hasSplitter = versionString.AsSpan().SplitOnce('+', out var left, out var right);
        left.SplitOnce('-', out var realVersion, out _);
        if (hasSplitter)
        {
            return new(realVersion.ToString(), right.ToString());
        }

        return new(realVersion.ToString(), null);
    }
}
