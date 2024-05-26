using Syndiesis.Utilities;
using System;

namespace Syndiesis;

public sealed class AppInfo
{
    public required InformationalVersion InformationalVersion { get; init; }

    public required InformationalVersion RoslynVersion { get; init; }
}
