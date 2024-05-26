using System.Text.Json;
using System.Text.Json.Serialization;

namespace Syndiesis;

[JsonSerializable(typeof(AppSettings), GenerationMode = JsonSourceGenerationMode.Default)]
public sealed partial class AppSettingsSerializationContext : JsonSerializerContext
{
    public static AppSettingsSerializationContext CustomDefault
    {
        get
        {
            return new(GetDefaultOptions());
        }
    }

    private static JsonSerializerOptions GetDefaultOptions()
    {
        return new()
        {
            IncludeFields = true,
            WriteIndented = true,
            AllowTrailingCommas = true,
        };
    }
}
