using Syndiesis.Utilities;

namespace Syndiesis;

public sealed class AppInfo
{
    public required InformationalVersion InformationalVersion { get; init; }

    public required InformationalVersion RoslynVersion { get; init; }
}
