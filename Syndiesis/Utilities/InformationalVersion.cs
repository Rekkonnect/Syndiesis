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
        if (hasSplitter)
        {
            return new(left.ToString(), right.ToString());
        }

        return new(versionString, null);
    }
}
