using System.Text.Json;
using System.Text.Json.Serialization;

namespace Syndiesis;

// Avoiding the usage of the source generator because we have SolidColorFieldGenerator
// generating the color fields, which are not included in the serialization generated
// code. This should not have any noticeable difference on performance.
public static class AppSettingsSerialization
{
    public static readonly JsonSerializerOptions DefaultOptions = GetDefaultOptions();

    private static JsonSerializerOptions GetDefaultOptions()
    {
        return new()
        {
            IncludeFields = true,
            WriteIndented = true,
            AllowTrailingCommas = true,
            IgnoreReadOnlyProperties = true,
            Converters =
            {
                new AvaloniaColorJsonConverter(),
            }
        };
    }

    public class AvaloniaColorJsonConverter : JsonConverter<Color>
    {
        public override Color Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType is not JsonTokenType.String)
                return default;
            return Avalonia.Media.Color.Parse(reader.GetString()!);
        }

        public override void Write(
            Utf8JsonWriter writer,
            Color value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
